// İki seçenekli kaydırmalı düğme (dil, rol, sekme). Seçili olanın arkasında "pill" kayar.

interface Option<T extends string> {
  value: T;
  label: string;
}

interface Props<T extends string> {
  options: [Option<T>, Option<T>];
  value: T;
  onChange: (value: T) => void;
  label: string;
  accent?: boolean; // seçili düğme turuncu
  tabs?: boolean; // sekme rolüyle (role="tablist")
  className?: string;
}

export default function Segmented<T extends string>({ options, value, onChange, label, accent, tabs, className }: Props<T>) {
  const index = options.findIndex((o) => o.value === value);
  return (
    <div className={`seg ${accent ? "accent" : ""} ${className ?? ""}`} role={tabs ? "tablist" : "group"} aria-label={label}>
      <span className="seg-pill" style={{ transform: `translateX(${index === 1 ? "100%" : "0"})` }} />
      {options.map((option) => {
        const selected = option.value === value;
        return (
          <button
            key={option.value}
            type="button"
            className={`seg-b ${selected ? "on" : ""}`}
            {...(tabs ? { role: "tab", "aria-selected": selected } : { "aria-pressed": selected })}
            onClick={() => onChange(option.value)}
          >
            {option.label}
          </button>
        );
      })}
    </div>
  );
}
