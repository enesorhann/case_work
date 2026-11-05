#!/usr/bin/env bash
# Codespace her açıldığında çalışır: uygulamayı http://localhost:8080 adresinde başlatır.
# QR kod adresi (Dpp:BaseUrl) Codespaces adresine Program.cs içinde otomatik ayarlanır.
cd "$(dirname "$0")/.."

# Arayüz derlenmemişse (ör. setup yarıda kaldıysa) derle
if [ ! -f wwwroot/index.html ]; then
  (cd frontend && npm ci && npm run build)
fi

# Telefonla QR okutulunca GitHub girişi istenmesin diye 8080 portunu herkese açık (Public) yapmayı dener.
make_port_public() {
  for _ in $(seq 1 60); do
    if curl -s -o /dev/null http://localhost:8080/; then
      if gh codespace ports visibility 8080:public -c "$CODESPACE_NAME" >/dev/null 2>&1; then
        echo "==> 8080 portu Public yapıldı: QR kod giriş istemeden açılır."
      else
        echo "==> 8080 portu otomatik Public yapılamadı. Ports sekmesinde 8080'e sağ tıklayıp"
        echo "    Port Visibility -> Public seçin (yalnızca QR kodu başka cihazla okutmak için gerekli)."
      fi
      return
    fi
    sleep 2
  done
}

if [ -n "$CODESPACE_NAME" ]; then
  echo "==> Uygulama adresi: https://$CODESPACE_NAME-8080.$GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN"
  make_port_public &
fi

exec dotnet run --project MiniDpp.csproj
