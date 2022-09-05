import api from "@/api/api-config";

export async function getSongsAxios() {
  return await api.get(`Songs/`);
}