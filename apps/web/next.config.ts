import path from "node:path";
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  turbopack: {
    // Raíz del monorepo, no de apps/web: aquí vive el node_modules hoisted
    // por los workspaces de npm.
    root: path.join(__dirname, "..", ".."),
  },
};

export default nextConfig;
