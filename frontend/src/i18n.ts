// Arayüz metinleri (TR / EN). Sunucudan gelen doğrulama mesajları Türkçedir.
import type { Lang } from "./types";

const tr = {
  brandSub: "Dijital Ürün Pasaportu", navLabel: "Ana menü", langLabel: "Dil seçimi", themeLabel: "Koyu temayı aç / kapat",
  nav: ["İçe aktar", "Ürünler", "Pasaport"],

  impEyebrow: "Adım 1 · CSV içe aktarma", impTitle: "Ürün verilerini yükleyin, gerisini biz doğrulayalım.",
  impLead: "Geçerli satırlar SQLite veritabanına kaydedilir. Hatalı satırlar kaydedilmez; her biri CSV satır numarası ve nedeniyle listelenir.",
  dropTitle: "CSV dosyasını buraya sürükleyin", dropSub: "veya cihazınızdan seçin · .csv (UTF-8 ya da Windows-1254, ayraç , veya ;)",
  pickFile: "Dosya seç", useSample: "Örnek CSV ile dene",
  background: "Arka planda işle (RabbitMQ)",
  excelWarn: "CSV dosyasını Excel’de açıp kaydetmeyin; GTIN başındaki sıfırlar silinebilir.",
  validating: "Satırlar doğrulanıyor…", validatingSub: "GTIN kontrol basamağı, elyaf oranları ve zorunlu alanlar kontrol ediliyor.",
  queued: "Arka plan işi çalışıyor…",
  fileMeta: (n: number) => `1 başlık + ${n} kayıt`, newFile: "Yeni dosya",
  fileError: "Dosya içe aktarılamadı",
  statTotal: "Toplam kayıt", statSaved: "Kaydedildi", statRejected: "Reddedildi",
  resultTitle: "Satır bazında sonuç", headerNote: "Başlık satırı = 1. satır", row: "Satır",
  status: { created: "Yeni kayıt", updated: "Güncellendi", unchanged: "Değişiklik yok" },
  rejected: "Reddedildi", viewProducts: "Kayıtlı ürünleri gör",
  rulesTitle: "Doğrulama kuralları",
  rules: [
    "GTIN boş olamaz; 14 haneli sayısal metin ve GS1 kontrol basamağı doğru olmalı.",
    "Pamuk ve elastan yüzdeleri sayısal, 0–100 aralığında ve toplamı %100 olmalı.",
    "Model adı, batch numarası ve üretim yeri boş olamaz.",
    "Karbon değeri sayısal ve negatif olmayan bir değer olmalı.",
  ],
  colsTitle: "Beklenen sütunlar", colsNote: "Sütun sırası önemli değil; urun_adi isteğe bağlıdır.",

  prodEyebrow: "Adım 2 · Veritabanı", prodTitle: "Kayıtlı ürünler",
  cntProducts: (n: number) => `${n} ürün`, cntModels: (n: number) => `${n} model`, cntBatches: (n: number) => `${n} batch`,
  emptyTitle: "Henüz kayıtlı ürün yok", emptyPassTitle: "Gösterilecek pasaport yok",
  emptySub: "Ürünleri görmek için önce bir CSV dosyası içe aktarın.", emptyCta: "CSV içe aktar",
  loadError: "Veriler yüklenemedi", retry: "Tekrar dene",

  back: "Ürünler", roleLabel: "Görünüm rolü", consumer: "Tüketici", authorized: "Yetkili",
  formatLabel: "Çıktı biçimi", tabHtml: "Pasaport", tabJson: "JSON-LD", openHtml: "HTML sayfasını aç",
  dppLabel: "Dijital Ürün Pasaportu", gs1Ok: "GS1 doğrulandı",
  fModel: "Model", fBatch: "Batch / Lot", fPlace: "Üretim yeri", fComp: "Elyaf bileşimi", fVersion: "Sürüm",
  cotton: "pamuk", elastane: "elastan", updated: "Güncelleme",
  carbonTitle: "Karbon ayak izi", carbonLocked: "Bu alan yalnızca yetkili görünümde gösterilir.",
  carbonAuthSub: "Yetkili görünüm · karbon_kg_co2e", switchAuth: "Yetkili görünüme geç",
  copy: "Kopyala",
  qrTitle: "QR kod", qrSub: "HTML pasaport adresine yönlendirir", qrAria: "Ürün pasaportu QR kodu",
  download: "PNG indir", copyLink: "Bağlantıyı kopyala",
  relTitle: "Model ve batch ilişkisi", otherProducts: "Diğer ürünler",
  versionsTitle: "Önceki sürümler", versionsNone: "Bu ürün henüz güncellenmedi.",
  toastLink: "Bağlantı kopyalandı", toastJson: "JSON-LD kopyalandı", toastCopyFail: "Kopyalanamadı",
  toastImport: (saved: number, rejected: number) => `${saved} satır kaydedildi, ${rejected} satır reddedildi`,
  footL: "Mini DPP · ESG Next teknik değerlendirme çalışması",
};

export type Texts = typeof tr;

const en: Texts = {
  brandSub: "Digital Product Passport", navLabel: "Main menu", langLabel: "Language", themeLabel: "Toggle dark theme",
  nav: ["Import", "Products", "Passport"],

  impEyebrow: "Step 1 · CSV import", impTitle: "Upload your product data, we’ll validate the rest.",
  impLead: "Valid rows are stored in the SQLite database. Invalid rows are not saved; each one is listed with its CSV line number and reason.",
  dropTitle: "Drag your CSV file here", dropSub: "or pick it from your device · .csv (UTF-8 or Windows-1254, delimiter , or ;)",
  pickFile: "Choose file", useSample: "Try the sample CSV",
  background: "Process in background (RabbitMQ)",
  excelWarn: "Don’t open and save the CSV in Excel; leading zeros in GTINs may be removed.",
  validating: "Validating rows…", validatingSub: "Checking GTIN check digits, fiber ratios and required fields.",
  queued: "Background job running…",
  fileMeta: (n: number) => `1 header + ${n} records`, newFile: "New file",
  fileError: "The file could not be imported",
  statTotal: "Total records", statSaved: "Saved", statRejected: "Rejected",
  resultTitle: "Result per row", headerNote: "Header row = line 1", row: "Line",
  status: { created: "New record", updated: "Updated", unchanged: "No change" },
  rejected: "Rejected", viewProducts: "View saved products",
  rulesTitle: "Validation rules",
  rules: [
    "GTIN is required; a 14-digit numeric string with a valid GS1 check digit.",
    "Cotton and elastane percentages are numeric, between 0–100, and sum to 100%.",
    "Model name, batch number and production place are required.",
    "Carbon value must be numeric and non-negative.",
  ],
  colsTitle: "Expected columns", colsNote: "Column order doesn’t matter; urun_adi is optional.",

  prodEyebrow: "Step 2 · Database", prodTitle: "Saved products",
  cntProducts: (n: number) => `${n} products`, cntModels: (n: number) => `${n} models`, cntBatches: (n: number) => `${n} batches`,
  emptyTitle: "No saved products yet", emptyPassTitle: "No passport to show",
  emptySub: "Import a CSV file first to see products here.", emptyCta: "Import CSV",
  loadError: "Data could not be loaded", retry: "Try again",

  back: "Products", roleLabel: "View role", consumer: "Consumer", authorized: "Authorized",
  formatLabel: "Output format", tabHtml: "Passport", tabJson: "JSON-LD", openHtml: "Open HTML page",
  dppLabel: "Digital Product Passport", gs1Ok: "GS1 verified",
  fModel: "Model", fBatch: "Batch / Lot", fPlace: "Production place", fComp: "Fiber composition", fVersion: "Version",
  cotton: "cotton", elastane: "elastane", updated: "Updated",
  carbonTitle: "Carbon footprint", carbonLocked: "Shown only in the authorized view.",
  carbonAuthSub: "Authorized view · karbon_kg_co2e", switchAuth: "Switch to authorized",
  copy: "Copy",
  qrTitle: "QR code", qrSub: "Points to the HTML passport URL", qrAria: "Product passport QR code",
  download: "Download PNG", copyLink: "Copy link",
  relTitle: "Model and batch relation", otherProducts: "Other products",
  versionsTitle: "Previous versions", versionsNone: "This product has not been updated yet.",
  toastLink: "Link copied", toastJson: "JSON-LD copied", toastCopyFail: "Could not copy",
  toastImport: (saved: number, rejected: number) => `${saved} rows saved, ${rejected} rows rejected`,
  footL: "Mini DPP · ESG Next technical assessment",
};

export const TEXTS: Record<Lang, Texts> = { tr, en };

// Türkçede "%98" ve "12,4", İngilizcede "98%" ve "12.4"
export function formatPercent(value: number, lang: Lang): string {
  const n = formatNumber(value, lang);
  return lang === "tr" ? `%${n}` : `${n}%`;
}

export function formatNumber(value: number, lang: Lang): string {
  return value.toLocaleString(lang === "tr" ? "tr-TR" : "en-US", { maximumFractionDigits: 3 });
}

export function formatDate(iso: string, lang: Lang): string {
  return new Date(iso).toLocaleString(lang === "tr" ? "tr-TR" : "en-GB", { dateStyle: "medium", timeStyle: "short" });
}
