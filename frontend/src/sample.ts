// "Örnek CSV ile dene": ödevle gelen dosya derleme sırasında arayüze gömülür (tek kaynak: data/ klasörü).
import sampleCsv from "../../data/urunler_duzeltilmis_test.csv?raw";

export const SAMPLE_FILE_NAME = "urunler_duzeltilmis_test.csv";

export function sampleFile(): File {
  return new File([sampleCsv], SAMPLE_FILE_NAME, { type: "text/csv" });
}
