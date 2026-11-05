import type { ReactNode } from "react";

interface Props {
  icon: ReactNode;
  title: string;
  text: string;
  actionLabel?: string;
  onAction?: () => void;
}

export default function EmptyState({ icon, title, text, actionLabel, onAction }: Props) {
  return (
    <div className="card empty stag">
      <span className="ic">{icon}</span>
      <h2 className="h2">{title}</h2>
      <p className="lead" style={{ fontSize: 15 }}>{text}</p>
      {actionLabel && onAction && (
        <button type="button" className="btn btn-p" onClick={onAction}>{actionLabel}</button>
      )}
    </div>
  );
}
