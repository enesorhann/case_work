// Backend ile konuşan tüm fonksiyonlar tek dosyada.
import type { ImportJob, ImportResult, ProductJsonLd, ProductSummary, ProductVersion, Role } from "./types";

async function readJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let message = `İstek başarısız (${response.status})`;
    try {
      const body = await response.json();
      if (body.detail) message = body.detail;
    } catch {
      // gövde JSON değil
    }
    throw new Error(message);
  }
  return response.json() as Promise<T>;
}

function csvForm(file: File): FormData {
  const form = new FormData();
  form.append("file", file);
  return form;
}

export function roleQuery(role: Role): string {
  return role === "yetkili" ? "?rol=yetkili" : "";
}

export const paths = {
  passportHtml: (gtin: string, role: Role) => `/dpp/${gtin}${roleQuery(role)}`,
  passportJsonLd: (gtin: string, role: Role) => `/dpp/${gtin}.jsonld${roleQuery(role)}`,
  qrPng: (gtin: string) => `/dpp/${gtin}/qr.png`,
};

export const api = {
  listProducts: () => fetch("/api/products").then((r) => readJson<ProductSummary[]>(r)),

  importCsv: (file: File) =>
    fetch("/api/import", { method: "POST", body: csvForm(file) }).then((r) => readJson<ImportResult>(r)),

  startImportJob: (file: File) =>
    fetch("/api/import/jobs", { method: "POST", body: csvForm(file) }).then((r) =>
      readJson<{ job_id: string; status_url: string }>(r),
    ),

  getImportJob: (jobId: string) => fetch(`/api/import/jobs/${jobId}`).then((r) => readJson<ImportJob>(r)),

  // JSON-LD hem ekranda gösterilir hem de ham metin olarak kopyalanır.
  getPassport: async (gtin: string, role: Role) => {
    const response = await fetch(paths.passportJsonLd(gtin, role));
    const text = await response.clone().text();
    const data = await readJson<ProductJsonLd>(response);
    return { data, text };
  },

  getVersions: (gtin: string) => fetch(`/api/products/${gtin}/versions`).then((r) => readJson<ProductVersion[]>(r)),
};
