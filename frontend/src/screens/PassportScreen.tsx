// Adım 3: ürün pasaportu. Veriler backend'in JSON-LD çıktısından okunur (GET /dpp/{gtin}.jsonld);
// böylece ekrandaki bilgiler makine tarafından okunan çıktıyla birebir aynıdır ve
// karbonu gizleme kuralı yalnızca backend'de uygulanır.
import { useEffect, useState } from "react";
import { api, paths } from "../api";
import { formatDate, formatNumber, type Texts } from "../i18n";
import type { Lang, ProductJsonLd, ProductSummary, ProductVersion, PropertyValue, Role } from "../types";
import Composition from "../components/Composition";
import EmptyState from "../components/EmptyState";
import Segmented from "../components/Segmented";
import {
  ArrowLeftIcon, BoxIcon, CheckIcon, CopyIcon, DownloadIcon, ExternalIcon, LayersIcon, LeafIcon, LinkIcon, LockIcon, LogoIcon,
} from "../components/Icons";

type Tab = "html" | "json";

interface Props {
  t: Texts;
  lang: Lang;
  products: ProductSummary[] | null;
  gtin: string | null;
  role: Role;
  onRole: (role: Role) => void;
  onOpen: (gtin: string) => void;
  onBack: () => void;
  onImport: () => void;
  onToast: (message: string) => void;
}

interface Loaded {
  data: ProductJsonLd;
  text: string;
  versions: ProductVersion[];
}

function values(data: ProductJsonLd, id: string): PropertyValue[] {
  return data.additionalProperty.filter((p) => p.propertyID === id);
}

async function copyText(text: string): Promise<boolean> {
  try {
    await navigator.clipboard.writeText(text);
    return true;
  } catch {
    return false;
  }
}

export default function PassportScreen({ t, lang, products, gtin, role, onRole, onOpen, onBack, onImport, onToast }: Props) {
  const [tab, setTab] = useState<Tab>("html");
  const [loaded, setLoaded] = useState<Loaded | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!gtin) return;
    let active = true;
    setError(null);
    Promise.all([api.getPassport(gtin, role), api.getVersions(gtin)])
      .then(([passport, versions]) => active && setLoaded({ ...passport, versions }))
      .catch((err: Error) => active && setError(err.message));
    return () => { active = false; };
  }, [gtin, role]);

  if (products !== null && products.length === 0) {
    return (
      <div className="screen">
        <EmptyState icon={<LogoIcon size={28} strokeWidth={2} />} title={t.emptyPassTitle} text={t.emptySub} actionLabel={t.emptyCta} onAction={onImport} />
      </div>
    );
  }
  if (!gtin) return <div className="screen"><div className="skeleton" /></div>;
  if (error) {
    return <div className="screen"><EmptyState icon={<LogoIcon size={28} strokeWidth={2} />} title={t.loadError} text={error} actionLabel={t.back} onAction={onBack} /></div>;
  }

  const auth = role === "yetkili";
  const htmlPath = paths.passportHtml(gtin, role);
  const data = loaded?.data.gtin14 === gtin ? loaded.data : null;

  return (
    <div className="screen">
      <div className="ptop">
        <button type="button" className="btn btn-g btn-sm" onClick={onBack}><ArrowLeftIcon size={16} />{t.back}</button>
        <a className="url mono" style={{ flex: "1 1 280px", textDecoration: "none", color: "var(--text)" }} href={htmlPath}
          target="_blank" rel="noreferrer" title={t.openHtml}>
          <LinkIcon size={14} style={{ flex: "none", color: "var(--muted)" }} />
          <span style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap", flex: 1 }}>
            <span className="host">{window.location.host}</span>/dpp/{gtin}{auth && <span className="q">?rol=yetkili</span>}
          </span>
          <ExternalIcon size={14} style={{ flex: "none", color: "var(--muted)" }} />
          <span className="sr-only">{t.openHtml}</span>
        </a>
        <Segmented accent label={t.roleLabel} value={role} onChange={onRole}
          options={[{ value: "tuketici", label: t.consumer }, { value: "yetkili", label: t.authorized }]} />
      </div>

      <div className="cols">
        <div className="col-main">
          <Segmented tabs label={t.formatLabel} value={tab} onChange={setTab} className="tabs"
            options={[{ value: "html", label: t.tabHtml }, { value: "json", label: t.tabJson }]} />

          {!data && <div className="skeleton" />}
          {data && loaded && tab === "html" && <PassportCard t={t} lang={lang} data={data} auth={auth} onAuth={() => onRole("yetkili")} />}
          {data && loaded && tab === "json" && (
            <JsonView t={t} data={data} path={paths.passportJsonLd(gtin, role)}
              onCopy={async () => onToast((await copyText(loaded.text)) ? t.toastJson : t.toastCopyFail)} />
          )}
        </div>

        <aside className="col-side">
          <div className="card qr stag" style={{ animationDelay: "80ms" }}>
            <div style={{ display: "flex", justifyContent: "space-between", width: "100%", alignItems: "center", gap: 8 }}>
              <b style={{ fontSize: 15 }}>{t.qrTitle}</b>
              <span className="badge soft">PNG</span>
            </div>
            <div className="qr-box"><img src={paths.qrPng(gtin)} alt={t.qrAria} width={192} height={192} /></div>
            <span className="muted" style={{ fontSize: 13, textAlign: "center" }}>{t.qrSub}</span>
            <div className="qr-actions">
              <a className="btn btn-p btn-sm" href={paths.qrPng(gtin)} download={`qr-${gtin}.png`}><DownloadIcon size={16} />{t.download}</a>
              <button type="button" className="btn btn-g btn-sm" disabled={!data}
                onClick={async () => data && onToast((await copyText(data.url)) ? t.toastLink : t.toastCopyFail)}>
                {t.copyLink}
              </button>
            </div>
          </div>

          {data && (
            <div className="card rel stag" style={{ animationDelay: "160ms" }}>
              <b style={{ fontSize: 15, marginBottom: 12 }}>{t.relTitle}</b>
              <div className="rel-n"><span className="ic"><LayersIcon /></span><div><b>{data.model.name}</b><span className="mono">ProductModels</span></div></div>
              <div className="rel-link"><i />1 → n</div>
              <div className="rel-n"><span className="ic"><BoxIcon /></span><div><b className="mono">{data.gtin14}</b><span className="mono">Products</span></div></div>
              <div className="rel-link"><i />1 → n</div>
              {batchesOf(data).map((batch) => (
                <div key={batch.number} className="rel-n" style={{ marginBottom: 6 }}>
                  <span className="ic"><BoxIcon /></span>
                  <div><b className="mono">{batch.number}</b><span className="mono">Batches · {batch.place}</span></div>
                </div>
              ))}
            </div>
          )}

          {loaded && data && (
            <div className="card stag" style={{ animationDelay: "200ms" }}>
              <div style={{ padding: "18px 20px 12px", fontSize: 15, fontWeight: 700 }}>{t.versionsTitle}</div>
              {loaded.versions.length === 0
                ? <p className="muted" style={{ margin: 0, padding: "0 20px 20px", fontSize: 13.5 }}>{t.versionsNone}</p>
                : (
                  <ul className="vlist">
                    {loaded.versions.map((version) => (
                      <li key={version.version_number}><b>v{version.version_number}</b><span className="muted">{formatDate(version.saved_at, lang)}</span></li>
                    ))}
                  </ul>
                )}
            </div>
          )}

          {products && products.length > 1 && (
            <div className="card stag" style={{ animationDelay: "240ms" }}>
              <div style={{ padding: "18px 20px 4px", fontSize: 15, fontWeight: 700 }}>{t.otherProducts}</div>
              <div className="plist">
                {products.map((product) => (
                  <button key={product.gtin} type="button" className={`pli ${product.gtin === gtin ? "on" : ""}`}
                    aria-current={product.gtin === gtin ? "page" : undefined} onClick={() => onOpen(product.gtin)}>
                    <span>{product.name}</span><span className="mono">…{product.gtin.slice(-6)}</span>
                  </button>
                ))}
              </div>
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}

// JSON-LD'de her parti için önce batch_no, ardından uretim_yeri gelir.
function batchesOf(data: ProductJsonLd) {
  const places = values(data, "uretim_yeri");
  return values(data, "batch_no").map((batch, index) => ({ number: String(batch.value), place: String(places[index]?.value ?? "") }));
}

function PassportCard({ t, lang, data, auth, onAuth }: { t: Texts; lang: Lang; data: ProductJsonLd; auth: boolean; onAuth: () => void }) {
  const cotton = Number(values(data, "elyaf_pamuk_yuzde")[0]?.value ?? 0);
  const elastane = Number(values(data, "elyaf_elastan_yuzde")[0]?.value ?? 0);
  const carbon = values(data, "karbon_kg_co2e")[0];
  const version = values(data, "dpp_surum")[0]?.value;
  const batches = batchesOf(data);

  return (
    <article className="card screen" style={{ overflow: "hidden" }}>
      <div className="pass-head">
        <div style={{ display: "flex", gap: 8, flexWrap: "wrap", alignItems: "center" }}>
          <span className="eyebrow">{t.dppLabel}</span>
          <span className="badge ok"><CheckIcon size={13} />{t.gs1Ok}</span>
        </div>
        <h1 className="pass-name">{data.name}</h1>
        <div className="gtin"><span>GTIN-14</span><b className="mono">{data.gtin14}</b></div>
        <div className="meta"><span>{t.fVersion} {version}</span><span>·</span><span>{t.updated}: {formatDate(data.dateModified, lang)}</span></div>
      </div>
      <div className="facts">
        <div className="fact stag" style={{ animationDelay: "60ms" }}><div className="l">{t.fModel}</div><div className="v">{data.model.name}</div></div>
        <div className="fact stag" style={{ animationDelay: "120ms" }}><div className="l">{t.fBatch}</div><div className="v mono" style={{ fontSize: 15 }}>{batches.map((b) => b.number).join(", ")}</div></div>
        <div className="fact stag" style={{ animationDelay: "180ms" }}><div className="l">{t.fPlace}</div><div className="v">{[...new Set(batches.map((b) => b.place))].join(", ")}</div></div>
      </div>
      <div className="sect">
        <div className="sect-t">{t.fComp}</div>
        <Composition cotton={cotton} elastane={elastane} lang={lang} t={t} height={14} />
      </div>
      {auth && carbon ? (
        <div className="carbon open" role="status">
          <span className="ic"><LeafIcon size={22} /></span>
          <div className="tx"><b>{t.carbonTitle}</b><span>{t.carbonAuthSub}</span></div>
          <div className="big">{formatNumber(Number(carbon.value), lang)}<small>kg CO₂e</small></div>
        </div>
      ) : (
        <div className="carbon locked">
          <span className="ic"><LockIcon size={20} /></span>
          <div className="tx"><b>{t.carbonTitle}</b><span>{t.carbonLocked}</span></div>
          <button type="button" className="btn btn-t btn-sm" onClick={onAuth}>{t.switchAuth}</button>
        </div>
      )}
    </article>
  );
}

// JSON-LD'yi satır satır, renklendirilmiş gösterir; yetkili rolde karbon satırları vurgulanır.
interface Token { c: "k" | "s" | "n" | "p"; v: string }
interface Line { pad: string; tokens: Token[]; highlight: boolean }

function toLines(value: unknown): Line[] {
  const lines: Line[] = [];
  const walk = (val: unknown, indent: number, keyTokens: Token[], comma: boolean, highlight: boolean) => {
    const isCarbon = !!val && typeof val === "object" && (val as PropertyValue).propertyID === "karbon_kg_co2e";
    const hl = highlight || isCarbon;
    const push = (tokens: Token[]) => lines.push({ pad: "  ".repeat(indent), tokens, highlight: hl });
    if (val && typeof val === "object") {
      const isArray = Array.isArray(val);
      const entries: [string | null, unknown][] = isArray ? (val as unknown[]).map((v) => [null, v]) : Object.entries(val);
      push([...keyTokens, { c: "p", v: isArray ? "[" : "{" }]);
      entries.forEach(([key, child], index) => {
        const childKey: Token[] = key === null ? [] : [{ c: "k", v: JSON.stringify(key) }, { c: "p", v: ": " }];
        walk(child, indent + 1, childKey, index < entries.length - 1, hl);
      });
      push([{ c: "p", v: (isArray ? "]" : "}") + (comma ? "," : "") }]);
    } else {
      const tokens: Token[] = [...keyTokens, { c: typeof val === "number" ? "n" : "s", v: JSON.stringify(val) }];
      if (comma) tokens.push({ c: "p", v: "," });
      push(tokens);
    }
  };
  walk(value, 0, [], false, false);
  return lines;
}

function JsonView({ t, data, path, onCopy }: { t: Texts; data: ProductJsonLd; path: string; onCopy: () => void }) {
  return (
    <div className="card screen" style={{ overflow: "hidden" }}>
      <div className="code-h">
        <div style={{ display: "flex", flexDirection: "column", gap: 2, minWidth: 0 }}>
          <b style={{ fontSize: 15 }}>JSON-LD</b>
          <a className="mono" style={{ fontSize: 12.5, color: "var(--muted)" }} href={path} target="_blank" rel="noreferrer">{path}</a>
        </div>
        <button type="button" className="btn btn-g btn-sm" onClick={onCopy}><CopyIcon size={16} />{t.copy}</button>
      </div>
      <pre className="code" aria-label="JSON-LD">
        {toLines(data).map((line, index) => (
          <span key={index} className={`ln ${line.highlight ? "hl" : ""}`}>
            <span>{line.pad}</span>
            {line.tokens.map((token, i) => <span key={i} className={token.c}>{token.v}</span>)}
          </span>
        ))}
      </pre>
    </div>
  );
}
