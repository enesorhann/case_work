// Adım 2: veritabanındaki ürünler (GET /api/products)
import type { Texts } from "../i18n";
import type { Lang, ProductSummary } from "../types";
import Composition from "../components/Composition";
import EmptyState from "../components/EmptyState";
import { ArrowRightIcon, BoxIcon, PinIcon } from "../components/Icons";

interface Props {
  t: Texts;
  lang: Lang;
  products: ProductSummary[] | null; // null: yükleniyor
  loadError: string | null;
  onRetry: () => void;
  onOpen: (gtin: string) => void;
  onImport: () => void;
}

export default function ProductsScreen({ t, lang, products, loadError, onRetry, onOpen, onImport }: Props) {
  const modelCount = new Set(products?.map((p) => p.model_name)).size;
  const batchCount = products?.reduce((sum, p) => sum + p.batches.length, 0) ?? 0;

  return (
    <div className="screen">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", gap: 16, flexWrap: "wrap", marginBottom: 24 }}>
        <div>
          <span className="eyebrow">{t.prodEyebrow}</span>
          <h1 className="h1" style={{ marginBottom: 0 }}>{t.prodTitle}</h1>
        </div>
        {products && products.length > 0 && (
          <div className="chips">
            <span className="chip">{t.cntProducts(products.length)}</span>
            <span className="chip">{t.cntModels(modelCount)}</span>
            <span className="chip">{t.cntBatches(batchCount)}</span>
          </div>
        )}
      </div>

      {loadError && (
        <EmptyState icon={<BoxIcon size={28} />} title={t.loadError} text={loadError} actionLabel={t.retry} onAction={onRetry} />
      )}

      {!loadError && products === null && <div className="pgrid"><div className="skeleton" /><div className="skeleton" /><div className="skeleton" /></div>}

      {!loadError && products?.length === 0 && (
        <EmptyState icon={<BoxIcon size={28} />} title={t.emptyTitle} text={t.emptySub} actionLabel={t.emptyCta} onAction={onImport} />
      )}

      {!loadError && products && products.length > 0 && (
        <div className="pgrid">
          {products.map((product, index) => {
            const latest = product.batches[product.batches.length - 1];
            return (
              <button key={product.gtin} type="button" className="pcard stag" style={{ animationDelay: `${index * 80}ms` }}
                onClick={() => onOpen(product.gtin)} aria-label={`${product.name}, GTIN ${product.gtin}`}>
                <div className="top">
                  <div>
                    <div className="name">{product.name}</div>
                    <div className="mono muted" style={{ fontSize: 13, marginTop: 4 }}>{product.gtin}</div>
                  </div>
                  <span className="arrow"><ArrowRightIcon /></span>
                </div>
                <div className="chips">
                  {latest && <span className="chip"><BoxIcon size={13} strokeWidth={2.2} />{latest.batch_number}{product.batches.length > 1 ? ` +${product.batches.length - 1}` : ""}</span>}
                  {latest && <span className="chip"><PinIcon size={13} />{latest.production_place}</span>}
                  <span className="chip">v{product.version}</span>
                </div>
                <Composition cotton={product.cotton_percent} elastane={product.elastane_percent} lang={lang} t={t} />
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
