import api from "@/api/api-config";

/*
   api-config üstünden api tarafında CRUD taleplerini gönderen fonksiyonları içerir
*/
export async function getSongs() {
  return await api.get("songs");
}

export async function deleteSong(id) {
  return await api.delete("songs/" + id);
}

export async function addSong(newSong) {
    return await api.post("songs", newSong);
}