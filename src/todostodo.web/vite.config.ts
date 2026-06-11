import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import { TanStackRouterVite } from "@tanstack/router-plugin/vite";
import path from "path";

export default defineConfig({
  plugins: [
    TanStackRouterVite({ routesDirectory: "./src/routes", generatedRouteTree: "./src/routeTree.gen.ts" }),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    proxy: {
      "/api": { target: "http://localhost:5162", secure: false },
      "/login": { target: "http://localhost:5162", secure: false },
      "/logout": { target: "http://localhost:5162", secure: false },
      "/register": { target: "http://localhost:5162", secure: false },
      "/manage": { target: "http://localhost:5162", secure: false },
    },
  },
});