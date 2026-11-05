import { useCallback, useEffect, useRef, useState } from "react";
import { api } from "./api";
import { TEXTS } from "./i18n";
import { sampleFile } from "./sample";
import type { ImportResult, Lang, ProductSummary, Role, Screen, Theme } from "./types";
import Header from "./components/Header";
import { CheckIcon } from "./components/Icons";
import ImportScreen, { type ImportState } from "./screens/ImportScreen";
import ProductsScreen from "./screens/ProductsScreen";
import PassportScreen from "./screens/PassportScreen";

// Adres çubuğu: #/ (içe aktar), #/urunler, #/pasaport/{gtin}
interface Route { screen: Screen; gtin: string | null }

function readRoute(): Route {
  const parts = window.location.hash.replace(/^#\/?/, "").split("/");
  if (parts[0] === "urunler") return { screen: "products", gtin: null };
  if (parts[0] === "pasaport") return { screen: "passport", gtin: parts[1] || null };
  return { screen: "import", gtin: null };
}

function writeRoute(route: Route) {
  const hash = route.screen === "products" ? "#/urunler"
    : route.screen === "passport" ? `#/pasaport/${route.gtin ?? ""}` : "#/";
  if (window.location.hash !== hash) window.history.pushState(null, "", hash);
}

// Dil ve tema tarayıcıda hatırlanır (gizli pencerede depolama kapalı olabilir).
function stored<T extends string>(key: string, allowed: T[], fallback: T): T {
  try {
    const value = localStorage.getItem(key) as T | null;
    return value && allowed.includes(value) ? value : fallback;
  } catch {
    return fallback;
  }
}

function store(key: string, value: string) {
  try { localStorage.setItem(key, value); } catch { /* depolama kapalı */ }
}

const POLL_MS = 1000;
const MAX_POLLS = 60;
const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export default function App() {
  const [route, setRoute] = useState<Route>(readRoute);
  const [lang, setLang] = useState<Lang>(() => stored("dpp-lang", ["tr", "en"], "tr"));
  const [theme, setTheme] = useState<Theme>(() =>
    stored("dpp-theme", ["light", "dark"], window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : "light"));
  const [role, setRole] = useState<Role>("tuketici");
  const [products, setProducts] = useState<ProductSummary[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [importState, setImportState] = useState<ImportState>({ status: "idle" });
  const [background, setBackground] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const toastTimer = useRef<number>();

  const t = TEXTS[lang];

  useEffect(() => { store("dpp-lang", lang); document.documentElement.lang = lang; }, [lang]);
  useEffect(() => { store("dpp-theme", theme); document.body.classList.toggle("dark", theme === "dark"); }, [theme]);

  useEffect(() => {
    const onPop = () => setRoute(readRoute());
    window.addEventListener("popstate", onPop);
    return () => window.removeEventListener("popstate", onPop);
  }, []);

  const loadProducts = useCallback(async () => {
    try {
      setLoadError(null);
      setProducts(await api.listProducts());
    } catch (error) {
      setLoadError((error as Error).message);
    }
  }, []);

  useEffect(() => { loadProducts(); }, [loadProducts]);

  function navigate(next: Route) {
    writeRoute(next);
    setRoute(next);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function showToast(message: string) {
    window.clearTimeout(toastTimer.current);
    setToast(message);
    toastTimer.current = window.setTimeout(() => setToast(null), 2400);
  }

  async function runImport(file: File) {
    setImportState({ status: "loading", fileName: file.name, background });
    try {
      const result = background ? await runInBackground(file) : await api.importCsv(file);
      setImportState({ status: "done", fileName: file.name, result });
      showToast(t.toastImport(result.saved_count, result.error_count));
      loadProducts();
    } catch (error) {
      setImportState({ status: "error", fileName: file.name, message: (error as Error).message });
    }
  }

  // RabbitMQ kuyruğuna gönderir, iş bitene kadar durumunu sorar.
  async function runInBackground(file: File): Promise<ImportResult> {
    const { job_id } = await api.startImportJob(file);
    for (let poll = 0; poll < MAX_POLLS; poll++) {
      const job = await api.getImportJob(job_id);
      if (job.status === "SUCCESS" && job.result) return job.result;
      if (job.status === "FAILURE") throw new Error(job.error ?? "Arka plan işi başarısız oldu.");
      await wait(POLL_MS);
    }
    throw new Error("İş zaman aşımına uğradı. RabbitMQ tüketicisi çalışıyor mu?");
  }

  // Pasaport ekranında GTIN verilmemişse ilk ürün gösterilir.
  const passportGtin = route.gtin ?? products?.[0]?.gtin ?? null;
  const hasProducts = !!products && products.length > 0;

  return (
    <div className={`app ${theme}`} id="top">
      <Header t={t} screen={route.screen} hasProducts={hasProducts}
        onNavigate={(screen) => navigate({ screen, gtin: screen === "passport" ? passportGtin : null })}
        lang={lang} onLang={setLang} theme={theme} onToggleTheme={() => setTheme(theme === "dark" ? "light" : "dark")} />

      <main className="main">
        <div className="wrap">
          {route.screen === "import" && (
            <ImportScreen t={t} state={importState} background={background} onBackgroundChange={setBackground}
              onImport={runImport} onSample={() => runImport(sampleFile())}
              onReset={() => setImportState({ status: "idle" })}
              onViewProducts={() => navigate({ screen: "products", gtin: null })} />
          )}

          {route.screen === "products" && (
            <ProductsScreen t={t} lang={lang} products={products} loadError={loadError} onRetry={loadProducts}
              onOpen={(gtin) => navigate({ screen: "passport", gtin })}
              onImport={() => navigate({ screen: "import", gtin: null })} />
          )}

          {route.screen === "passport" && (
            <PassportScreen t={t} lang={lang} products={products} gtin={passportGtin} role={role} onRole={setRole}
              onOpen={(gtin) => navigate({ screen: "passport", gtin })}
              onBack={() => navigate({ screen: "products", gtin: null })}
              onImport={() => navigate({ screen: "import", gtin: null })}
              onToast={showToast} />
          )}
        </div>
      </main>

      <footer className="foot">
        <div className="wrap" style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
          <span>{t.footL}</span>
          <span className="mono">ASP.NET Core · SQLite · Redis · RabbitMQ · React</span>
        </div>
      </footer>

      {toast && (
        <div className="toast" role="status" aria-live="polite"><CheckIcon size={16} />{toast}</div>
      )}
    </div>
  );
}
