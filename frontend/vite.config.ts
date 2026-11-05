import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Geliştirme sırasında API istekleri ASP.NET Core API'ye (8080) yönlendirilir.
const backend = "http://localhost:8080";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // "Örnek CSV ile dene" için ../data klasöründeki dosya okunabilsin
    fs: { allow: [".."] },
    proxy: {
      "/api": backend,
      "/dpp": backend,
    },
  },
  // Derleme çıktısı ASP.NET Core'un statik dosya klasörüne yazılır.
  build: { outDir: "../wwwroot", emptyOutDir: true },
});
