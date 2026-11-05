// Elyaf bileşimi çubuğu ve açıklaması (pamuk / elastan)
import { formatPercent, type Texts } from "../i18n";
import type { Lang } from "../types";

interface Props {
  cotton: number;
  elastane: number;
  lang: Lang;
  t: Texts;
  height?: number;
}

export default function Composition({ cotton, elastane, lang, t, height }: Props) {
  return (
    <>
      <div className="comp" style={height ? { height } : undefined} aria-hidden="true">
        <span className="c1" style={{ width: `${cotton}%` }} />
        <span className="c2" style={{ width: `${elastane}%`, animationDelay: "200ms" }} />
      </div>
      <div className="legend">
        <span><i style={{ background: "var(--primary)" }} />{formatPercent(cotton, lang)} {t.cotton}</span>
        <span><i style={{ background: "var(--seg2)" }} />{formatPercent(elastane, lang)} {t.elastane}</span>
      </div>
    </>
  );
}
