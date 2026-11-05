// Üst çubuk: logo, üç adımlı menü, dil seçimi ve tema düğmesi
import type { Lang, Screen, Theme } from "../types";
import type { Texts } from "../i18n";
import Segmented from "./Segmented";
import { LogoIcon } from "./Icons";

const SCREENS: Screen[] = ["import", "products", "passport"];

interface Props {
  t: Texts;
  screen: Screen;
  onNavigate: (screen: Screen) => void;
  hasProducts: boolean;
  lang: Lang;
  onLang: (lang: Lang) => void;
  theme: Theme;
  onToggleTheme: () => void;
}

export default function Header({ t, screen, onNavigate, hasProducts, lang, onLang, theme, onToggleTheme }: Props) {
  return (
    <header className="hdr">
      <div className="wrap hdr-in">
        <a className="brand" href="#/" aria-label="Mini DPP" onClick={(e) => { e.preventDefault(); onNavigate("import"); }}>
          <span className="brand-mark"><LogoIcon size={20} /></span>
          <span className="brand-t"><b>Mini DPP</b><span>{t.brandSub}</span></span>
        </a>

        <nav className="nav" aria-label={t.navLabel}>
          {SCREENS.map((item, index) => {
            const current = item === screen;
            const done = index === 0 && hasProducts && !current;
            return (
              <button key={item} type="button" className={`nav-b ${current ? "on" : ""} ${done ? "done" : ""}`}
                aria-current={current ? "page" : undefined} onClick={() => onNavigate(item)}>
                <span className="nav-n">{index + 1}</span>
                <span className="nav-l">{t.nav[index]}</span>
              </button>
            );
          })}
        </nav>

        <div className="ctrls">
          <Segmented label={t.langLabel} value={lang} onChange={onLang}
            options={[{ value: "tr", label: "TR" }, { value: "en", label: "EN" }]} />
          <button type="button" className={`tt ${theme}`} aria-label={t.themeLabel} aria-pressed={theme === "dark"} onClick={onToggleTheme}>
            <span className="tt-track">
              <svg className="tt-ico tt-sun" viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true"><circle cx="12" cy="12" r="4.5" stroke="none" /><path d="M12 1.8v2.4M12 19.8v2.4M1.8 12h2.4M19.8 12h2.4M4.8 4.8l1.7 1.7M17.5 17.5l1.7 1.7M4.8 19.2l1.7-1.7M17.5 6.5l1.7-1.7" fill="none" /></svg>
              <svg className="tt-ico tt-moon" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M20.5 14.2A8.5 8.5 0 0 1 9.8 3.5a8.5 8.5 0 1 0 10.7 10.7z" /><circle cx="17.5" cy="5" r="1.1" /><circle cx="20.6" cy="9" r=".8" /></svg>
              <span className="tt-knob" />
            </span>
          </button>
        </div>
      </div>
    </header>
  );
}
