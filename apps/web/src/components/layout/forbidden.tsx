export function Forbidden({ message }: { message: string }) {
  return (
    <div className="text-muted-foreground rounded-xl border border-dashed p-8 text-center text-sm">
      {message}
    </div>
  );
}
