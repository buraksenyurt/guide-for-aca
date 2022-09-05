import * as actionTypes from "./action-types";
import { getSongs, deleteSong, addSong } from "@/store/song/services";

export async function getSongsAction({ commit }) {
    // Şarkıların yüklendiğine dair bir durum bildiriyor
    commit(actionTypes.LOADING_SONGS, true);

    try {
        // servis fonksiyonundan veri çekiliyor
        const { data } = await getSongs();
        // şarkı listesinin alınması farklı bir durum ve payload olarak da listenin kendisi bildiriliyor
        commit(actionTypes.GET_SONGS, data.songList);
    } catch (e) {
        console.log(e);
    }
    // Şarkıların yüklenme durumu sona erdiği için false ile bir durum bilgilendirilmesi yapılıyor
    commit(actionTypes.LOADING_SONGS, false);
}

// Listeden şarkı çıkarmak için kullanılan fonksiyon
export async function removeSongAction({ commit }, payload) {
    commit(actionTypes.LOADING_SONGS, true);

    try {
        await deleteSong(payload);
        commit(actionTypes.REMOVE_SONG, payload);
    } catch (e) {
        console.log(e);
    }

    commit(actionTypes.LOADING_SONGS, false);
}

// Yeni bir şarkı eklemek için kullanılan fonksiyon
export async function addSongAction({ commit }, payload) {
    var langs = {
        English: 0,
        Turkish: 1,
        Spanish: 2
    };
    switch (payload.language.id) {
        case 0:
            payload.language = langs.English;
            break;
        case 1:
            payload.language = langs.Turkish;
            break;
        case 2:
            payload.language = langs.Spanish;
            break;
        default:
            payload.language = langs.Turkish;
    }
    commit(actionTypes.LOADING_SONGS, true);

    try {
        const { data } = await addSong(payload);
        payload.id = data;
        commit(actionTypes.ADD_SONG, payload);
    } catch (e) {
        console.log(e);
    }

    commit(actionTypes.LOADING_SONGS, false);
}