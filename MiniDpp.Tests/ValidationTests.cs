using MiniDpp.Exceptions;
using MiniDpp.Helpers;

namespace MiniDpp.Tests
{
    public class ValidationTests
    {
        private static readonly Dictionary<string, string> Valid = new()
        {
            ["gtin"] = "08691234500019",
            ["model_adi"] = "Slim Fit 501",
            ["batch_no"] = "B1",
            ["uretim_yeri"] = "Denizli",
            ["elyaf_pamuk_yuzde"] = "98",
            ["elyaf_elastan_yuzde"] = "2",
            ["karbon_kg_co2e"] = "12.4",
        };

        private static List<string> ErrorsFor(params (string Key, string Value)[] changes)
        {
            var values = new Dictionary<string, string>(Valid);
            foreach (var (key, value) in changes) values[key] = value;
            return ProductRowValidator.Validate(values, 2).Errors;
        }

        [Fact]
        public void Gs1CheckDigit()
        {
            Assert.Equal(9, GtinHelper.CalculateCheckDigit("0869123450001"));
            Assert.True(GtinHelper.IsValid("08691234500019"));
            Assert.False(GtinHelper.IsValid("08691234500018"));
        }

        [Fact]
        public void ValidRowKeepsLeadingZero()
        {
            var (row, errors) = ProductRowValidator.Validate(Valid, 2);
            Assert.Empty(errors);
            Assert.Equal("08691234500019", row!.Gtin);
        }

        [Fact]
        public void GtinRules()
        {
            Assert.Contains("boş", ErrorsFor(("gtin", ""))[0]);
            Assert.Contains("14 haneli", ErrorsFor(("gtin", "8691234500019"))[0]);
            Assert.Contains("rakam", ErrorsFor(("gtin", "8.69123E+12"))[0]);
            Assert.Contains("kontrol basamağı", ErrorsFor(("gtin", "08691234500018"))[0]);
        }

        [Fact]
        public void FiberRules()
        {
            Assert.Contains("sayısal", ErrorsFor(("elyaf_pamuk_yuzde", "yüz"))[0]);
            Assert.Contains("0-100", ErrorsFor(("elyaf_pamuk_yuzde", "120"), ("elyaf_elastan_yuzde", "-20"))[0]);
            Assert.Contains("toplamı 100", ErrorsFor(("elyaf_pamuk_yuzde", "90"), ("elyaf_elastan_yuzde", "5"))[0]);
            Assert.Empty(ErrorsFor(("elyaf_pamuk_yuzde", "97,5"), ("elyaf_elastan_yuzde", "2,5")));
        }

        [Fact]
        public void RequiredTextFieldsAndCarbon()
        {
            Assert.Equal(4, ErrorsFor(("model_adi", " "), ("batch_no", ""), ("uretim_yeri", ""), ("karbon_kg_co2e", "-1")).Count);
            Assert.Contains("sayısal", ErrorsFor(("karbon_kg_co2e", "abc"))[0]);
            Assert.Empty(ErrorsFor(("karbon_kg_co2e", "0")));
        }

        [Fact]
        public void LineNumbersStartAfterHeaderAndSemicolonCsvWorks()
        {
            var text = "GTIN;Model Adı;Batch No;Üretim Yeri;elyaf_pamuk_yuzde;elyaf_elastan_yuzde;karbon_kg_co2e\n"
                     + "08691234500019;M;B;Denizli;98;2;1\n\n08691234500026;M;B;İzmir;99;1;1\n";
            var rows = CsvFileReader.ReadRows(text);
            Assert.Equal(new[] { 2, 4 }, rows.Select(r => r.Line));
            Assert.Equal("08691234500019", rows[0].Values!["gtin"]);
        }

        [Fact]
        public void QuotedCellsMayContainDelimiter()
        {
            var text = "gtin,model_adi,batch_no,uretim_yeri,elyaf_pamuk_yuzde,elyaf_elastan_yuzde,karbon_kg_co2e\r\n"
                     + "08691234500019,M,B,\"Denizli, Türkiye\",98,2,1\r\n";
            Assert.Equal("Denizli, Türkiye", CsvFileReader.ReadRows(text)[0].Values!["uretim_yeri"]);
        }

        [Fact]
        public void MissingColumnsThrow()
        {
            var error = Assert.Throws<CsvFormatException>(() => CsvFileReader.ReadRows("gtin,model_adi\n1,2\n"));
            Assert.Contains("eksik", error.Message);
        }
    }
}
