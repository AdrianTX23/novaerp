"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { productsApi } from "@/lib/catalog-api";
import { ApiError } from "@/lib/api-client";
import type { ProductSummary } from "@/lib/types";
import { PackagePlus } from "lucide-react";

export function AdjustStockDialog({ product }: { product: ProductSummary }) {
  const [open, setOpen] = useState(false);
  const [delta, setDelta] = useState("");
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (value: number) => productsApi.adjustStock(product.id, value),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
      toast.success("Stock actualizado.");
      setOpen(false);
      setDelta("");
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo ajustar el stock.";
      toast.error(message);
    },
  });

  const parsedDelta = Number(delta);
  const isValid = delta !== "" && !Number.isNaN(parsedDelta) && parsedDelta !== 0;

  return (
    <Dialog open={open} onOpenChange={(next) => { setOpen(next); if (!next) setDelta(""); }}>
      <DialogTrigger render={<Button variant="ghost" size="icon-sm"><PackagePlus /></Button>} />
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Ajustar stock de {product.name}</DialogTitle>
          <DialogDescription>
            Stock actual: {product.quantityOnHand} {product.unitOfMeasure}. Un número positivo suma, uno negativo resta.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-1.5">
          <Label htmlFor="delta">Cantidad</Label>
          <Input
            id="delta"
            type="number"
            step="0.001"
            placeholder="ej. 10 o -5"
            value={delta}
            onChange={(e) => setDelta(e.target.value)}
          />
        </div>

        <DialogFooter>
          <Button disabled={!isValid || mutation.isPending} onClick={() => mutation.mutate(parsedDelta)}>
            {mutation.isPending ? "Guardando…" : "Aplicar ajuste"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
