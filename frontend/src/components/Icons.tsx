// Tasarımdaki çizgi ikonlar (dekoratif: aria-hidden)
import type { SVGProps } from "react";

type IconProps = SVGProps<SVGSVGElement> & { size?: number };

function Svg({ size = 18, children, strokeWidth = 2, ...rest }: IconProps) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={strokeWidth}
      strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" {...rest}>
      {children}
    </svg>
  );
}

export const LogoIcon = (p: IconProps) => <Svg strokeWidth={2.2} {...p}><rect x="4" y="3" width="16" height="18" rx="3" /><path d="M8 8h3v3H8z" /><path d="M14 8h2M14 11h2M8 15h8" /></Svg>;
export const UploadIcon = (p: IconProps) => <Svg {...p}><path d="M12 16V4" /><path d="m7 9 5-5 5 5" /><path d="M4 16v3a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-3" /></Svg>;
export const WarnIcon = (p: IconProps) => <Svg {...p}><path d="M12 9v4M12 17h.01" /><path d="M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z" /></Svg>;
export const FileIcon = (p: IconProps) => <Svg {...p}><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" /><path d="M14 2v6h6M8 13h8M8 17h5" /></Svg>;
export const AlertIcon = (p: IconProps) => <Svg strokeWidth={2.4} {...p}><circle cx="12" cy="12" r="9" /><path d="M12 8v5M12 16h.01" /></Svg>;
export const XIcon = (p: IconProps) => <Svg strokeWidth={3} {...p}><path d="M18 6 6 18M6 6l12 12" /></Svg>;
export const CheckIcon = (p: IconProps) => <Svg strokeWidth={3} {...p}><path d="m5 12 5 5 9-10" /></Svg>;
export const ArrowRightIcon = (p: IconProps) => <Svg strokeWidth={2.2} {...p}><path d="M5 12h14M13 6l6 6-6 6" /></Svg>;
export const ArrowLeftIcon = (p: IconProps) => <Svg strokeWidth={2.2} {...p}><path d="M19 12H5M11 18l-6-6 6-6" /></Svg>;
export const BoxIcon = (p: IconProps) => <Svg {...p}><path d="M21 8 12 3 3 8v8l9 5 9-5z" /><path d="M3 8l9 5 9-5M12 13v8" /></Svg>;
export const PinIcon = (p: IconProps) => <Svg strokeWidth={2.2} {...p}><path d="M12 21s7-6.2 7-12a7 7 0 0 0-14 0c0 5.8 7 12 7 12z" /><circle cx="12" cy="9" r="2.5" /></Svg>;
export const LinkIcon = (p: IconProps) => <Svg strokeWidth={2.2} {...p}><path d="M10 14a5 5 0 0 0 7 0l3-3a5 5 0 0 0-7-7l-1 1" /><path d="M14 10a5 5 0 0 0-7 0l-3 3a5 5 0 0 0 7 7l1-1" /></Svg>;
export const LeafIcon = (p: IconProps) => <Svg {...p}><path d="M11 20A7 7 0 0 1 9.8 6.1C15.5 5 17 4.5 19 2c1 2 2 4.2 2 8 0 5.5-4.8 10-10 10z" /><path d="M2 21c0-3 1.9-5.4 5.2-6" /></Svg>;
export const LockIcon = (p: IconProps) => <Svg {...p}><rect x="4" y="11" width="16" height="10" rx="2" /><path d="M8 11V7a4 4 0 0 1 8 0v4" /></Svg>;
export const CopyIcon = (p: IconProps) => <Svg {...p}><rect x="9" y="9" width="12" height="12" rx="2" /><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" /></Svg>;
export const DownloadIcon = (p: IconProps) => <Svg strokeWidth={2.2} {...p}><path d="M12 4v12M7 11l5 5 5-5M4 20h16" /></Svg>;
export const LayersIcon = (p: IconProps) => <Svg {...p}><path d="M20.4 7.4 12 3 3.6 7.4 12 12z" /><path d="M3.6 12 12 16.4 20.4 12M3.6 16.6 12 21l8.4-4.4" /></Svg>;
export const ExternalIcon = (p: IconProps) => <Svg strokeWidth={2.2} {...p}><path d="M14 4h6v6M20 4l-9 9" /><path d="M19 14v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1h5" /></Svg>;
