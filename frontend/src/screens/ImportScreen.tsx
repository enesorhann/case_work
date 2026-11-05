// Adım 1: CSV içe aktarma. Sürükle-bırak / dosya seç / örnek CSV, sonuçların satır satır gösterimi.
import { useRef, useState, type DragEvent } from "react";
import type { Texts } from "../i18n";
import type { ImportResult } from "../types";
import { AlertIcon, ArrowRightIcon, CheckIcon, FileIcon, UploadIcon, WarnIcon, XIcon } from "../components/Icons";

export type ImportState =
  | { status: "idle" }
  | { status: "loading"; fileName: string; background: boolean }
  | { status: "done"; fileName: string; result: ImportResult }
  | { status: "error"; fileName: string; message: string };

const COLUMNS = ["gtin", "model_adi", "batch_no", "uretim_yeri", "elyaf_pamuk_yuzde", "elyaf_elastan_yuzde", "karbon_kg_co2e"];

interface Props {
  t: Texts;
  state: ImportState;
  background: boolean;
  onBackgroundChange: (value: boolean) => void;
  onImport: (file: File) => void;
  onSample: () => void;
  onReset: () => void;
  onViewProducts: () => void;
}

export default function ImportScreen({ t, state, background, onBackgroundChange, onImport, onSample, onReset, onViewProducts }: Props) {
  return (
    <div className="screen cols">
      <div className="col-main">
        <div>
          <span className="eyebrow">{t.impEyebrow}</span>
          <h1 className="h1">{t.impTitle}</h1>
          <p className="lead">{t.impLead}</p>
        </div>

        {state.status === "idle" && (
          <DropZone t={t} background={background} onBackgroundChange={onBackgroundChange} onImport={onImport} onSample={onSample} />
        )}

        {state.status === "loading" && (
          <div className="card prog stag" role="status" aria-live="polite">
            <div className="prog-row">
              <span>{state.background ? t.queued : t.validating}</span>
              <span className="mono muted">{state.fileName}</span>
            </div>
            <div className="prog-bar"><div className="prog-fill" /></div>
            <span className="muted" style={{ fontSize: 13.5 }}>{t.validatingSub}</span>
          </div>
        )}

        {state.status === "error" && (
          <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
            <FileCard t={t} fileName={state.fileName} meta="" onReset={onReset} />
            <div className="err-card stag" role="alert">
              <AlertIcon size={20} />
              <div><b>{t.fileError}</b><p>{state.message}</p></div>
            </div>
          </div>
        )}

        {state.status === "done" && (
          <Results t={t} fileName={state.fileName} result={state.result} onReset={onReset} onViewProducts={onViewProducts} />
        )}
      </div>

      <aside className="col-side">
        <div className="card card-pad stag" style={{ animationDelay: "120ms" }}>
          <h2 className="h2" style={{ fontSize: 16 }}>{t.rulesTitle}</h2>
          <ul className="rules">
            {t.rules.map((rule, index) => (
              <li key={rule}><span className="k">{index + 1}</span><span>{rule}</span></li>
            ))}
          </ul>
        </div>
        <div className="card card-pad stag" style={{ animationDelay: "200ms" }}>
          <h2 className="h2" style={{ fontSize: 16 }}>{t.colsTitle}</h2>
          <div className="chips" style={{ marginTop: 12 }}>
            {COLUMNS.map((column) => <span key={column} className="chip mono">{column}</span>)}
          </div>
          <p className="muted" style={{ fontSize: 13, margin: "12px 0 0" }}>{t.colsNote}</p>
        </div>
      </aside>
    </div>
  );
}

function DropZone({ t, background, onBackgroundChange, onImport, onSample }: {
  t: Texts; background: boolean; onBackgroundChange: (v: boolean) => void; onImport: (f: File) => void; onSample: () => void;
}) {
  const [dragging, setDragging] = useState(false);
  const input = useRef<HTMLInputElement>(null);

  function handleDrop(event: DragEvent) {
    event.preventDefault();
    setDragging(false);
    const file = event.dataTransfer.files?.[0];
    if (file) onImport(file);
  }

  return (
    <div className="stag" style={{ display: "flex", flexDirection: "column", gap: 14 }}>
      <div className={`drop ${dragging ? "over" : ""}`}
        onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
        onDragLeave={() => setDragging(false)}
        onDrop={handleDrop}>
        <span className="drop-ico"><UploadIcon size={28} /></span>
        <h3>{t.dropTitle}</h3>
        <p>{t.dropSub}</p>
        <div className="drop-actions">
          <input ref={input} id="csvfile" className="sr-only" type="file" accept=".csv,text/csv"
            onChange={(e) => { const file = e.target.files?.[0]; if (file) onImport(file); e.target.value = ""; }} />
          <label htmlFor="csvfile" className="btn btn-p" style={{ cursor: "pointer" }} tabIndex={0}
            onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); input.current?.click(); } }}>
            {t.pickFile}
          </label>
          <button type="button" className="btn btn-g" onClick={onSample}>{t.useSample}</button>
        </div>
        <label className="switch">
          <input type="checkbox" checked={background} onChange={(e) => onBackgroundChange(e.target.checked)} />
          {t.background}
        </label>
      </div>
      <div className="warn"><WarnIcon /><span>{t.excelWarn}</span></div>
    </div>
  );
}

function FileCard({ t, fileName, meta, onReset }: { t: Texts; fileName: string; meta: string; onReset: () => void }) {
  return (
    <div className="card file stag">
      <span className="file-ico"><FileIcon size={22} /></span>
      <div className="file-t"><b className="mono">{fileName}</b>{meta && <span>{meta}</span>}</div>
      <button type="button" className="btn btn-t btn-sm" onClick={onReset}>{t.newFile}</button>
    </div>
  );
}

// Kaydedilen ve reddedilen satırlar CSV'deki sırayla tek listede gösterilir.
function Results({ t, fileName, result, onReset, onViewProducts }: {
  t: Texts; fileName: string; result: ImportResult; onReset: () => void; onViewProducts: () => void;
}) {
  const rows = [
    ...result.saved.map((row) => ({ line: row.line, gtin: row.gtin, model: row.model_name, status: row.status, errors: [] as string[] })),
    ...result.errors.map((row) => ({ line: row.line, gtin: row.gtin, model: row.model_name, status: null, errors: row.errors })),
  ].sort((a, b) => a.line - b.line);

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
      <FileCard t={t} fileName={fileName} meta={t.fileMeta(result.total_rows)} onReset={onReset} />

      <div className="stats">
        <div className="stat stag" style={{ animationDelay: "60ms" }}><div className="v">{result.total_rows}</div><div className="l">{t.statTotal}</div></div>
        <div className="stat ok stag" style={{ animationDelay: "120ms" }}><div className="v">{result.saved_count}</div><div className="l">{t.statSaved}</div></div>
        <div className="stat err stag" style={{ animationDelay: "180ms" }}><div className="v">{result.error_count}</div><div className="l">{t.statRejected}</div></div>
      </div>

      <div className="card stag" style={{ animationDelay: "220ms", overflow: "hidden" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 12, padding: "18px 20px", borderBottom: "1px solid var(--border)", flexWrap: "wrap" }}>
          <h2 className="h2" style={{ fontSize: 17 }}>{t.resultTitle}</h2>
          <span className="muted" style={{ fontSize: 13 }}>{t.headerNote}</span>
        </div>
        <div className="rlist">
          {rows.map((row, index) => (
            <div key={row.line} className="rrow stag" style={{ animationDelay: `${260 + index * 70}ms` }}>
              <div className="rnum">{t.row} <b>{row.line}</b></div>
              <div>
                <div className="rt">{row.model || "—"}</div>
                <div className="rg mono">{row.gtin || "—"}</div>
                {row.errors.length > 0 && (
                  <ul className="errs">
                    {row.errors.map((message) => (
                      <li key={message}><AlertIcon size={14} /><span>{message}</span></li>
                    ))}
                  </ul>
                )}
              </div>
              {row.status === null
                ? <span className="badge err"><XIcon size={13} />{t.rejected}</span>
                : <span className={`badge ${row.status === "unchanged" ? "neutral" : "ok"}`}><CheckIcon size={13} />{t.status[row.status]}</span>}
            </div>
          ))}
        </div>
      </div>

      {result.saved_count > 0 && (
        <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
          <button type="button" className="btn btn-p" onClick={onViewProducts}>{t.viewProducts}<ArrowRightIcon /></button>
        </div>
      )}
    </div>
  );
}
