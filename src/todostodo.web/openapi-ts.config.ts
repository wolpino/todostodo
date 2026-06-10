import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "http://localhost:5162/swagger/v1/swagger.json",
  output: {
    path: "src/api/generated",
    format: "prettier",
  },
  plugins: ["@hey-api/client-fetch"],
});
