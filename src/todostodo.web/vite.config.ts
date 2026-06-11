import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
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