// Backend'in döndürdüğü veri tipleri (Dtos/ klasöründeki C# sınıflarının snake_case karşılıkları)

export type Role = "tuketici" | "yetkili";
export type Lang = "tr" | "en";
export type Theme = "light" | "dark";
export type Screen = "import" | "products" | "passport";

export interface Batch {
  batch_number: string;
  production_place: string;
}

export interface ProductSummary {
  gtin: string;
  name: string;
  model_name: string;
  cotton_percent: number;
  elastane_percent: number;
  batches: Batch[];
  version: number;
  updated_at: string;
}

export interface SavedRow {
  line: number;
  gtin: string;
  model_name: string;
  status: "created" | "updated" | "unchanged";
}

export interface RowError {
  line: number;
  gtin: string;
  model_name: string;
  errors: string[];
}

export interface ImportResult {
  total_rows: number;
  saved_count: number;
  error_count: number;
  saved: SavedRow[];
  errors: RowError[];
}

export interface ImportJob {
  job_id: string;
  status: "PENDING" | "STARTED" | "SUCCESS" | "FAILURE";
  result: ImportResult | null;
  error: string | null;
}

export interface ProductVersion {
  version_number: number;
  saved_at: string;
  data: Record<string, unknown>;
}

// schema.org/Product JSON-LD çıktısı
export interface PropertyValue {
  "@type": "PropertyValue";
  propertyID: string;
  name: string;
  value: string | number;
  unitText?: string;
}

export interface ProductJsonLd {
  "@context": string;
  "@type": "Product";
  "@id": string;
  url: string;
  name: string;
  gtin14: string;
  model: { "@type": "ProductModel"; name: string };
  material: string;
  additionalProperty: PropertyValue[];
  dateModified: string;
}
