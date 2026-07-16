import Link from "next/link";
import { Button } from "@/components/ui/button";
import { FileQuestion } from "lucide-react";

export default function NotFound() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-4 p-6 text-center">
      <FileQuestion className="text-muted-foreground size-10" strokeWidth={1.5} />
      <div className="space-y-1">
        <h1 className="text-lg font-semibold">Página no encontrada</h1>
        <p className="text-muted-foreground text-sm">
          La página que buscas no existe o fue movida.
        </p>
      </div>
      <Button render={<Link href="/dashboard" />}>Ir al inicio</Button>
    </div>
  );
}
