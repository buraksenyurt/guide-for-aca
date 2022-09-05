import * as actionTypes from "./action-types";

const mutations = {
  [actionTypes.GET_SONGS](state, songs) {
    state.songs = songs;
  },

  [actionTypes.LOADING_SONGS](state, value) {
    state.loading = value;
  },

  [actionTypes.REMOVE_SONG](state, id) {
    state.songs = state.songs.filter((tl) => tl.id !== id);
  },

  [actionTypes.ADD_SONG](state, newSong) {
    state.songs.unshift(newSong);
  },
};

export default mutations;
