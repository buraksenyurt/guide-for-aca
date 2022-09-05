<template>
    <v-container>
      <div class="text-h4 mb-10">Beste Listesi</div>
      <div class="v-picker--full-width d-flex justify-center" v-if="loading">
        <v-progress-circular
          :size="70"
          :width="7"
          color="purple"
          indeterminate
        ></v-progress-circular>
      </div>
  
      <v-simple-table>
        <template v-slot:default>
          <thead>
            <tr>
              <th>Title</th>
              <th>Language</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="b in songs" :key="b.id">
              <td>{{ b.title }}</td>
              <td>{{ getLang(b.language) }}</td>
            </tr>
          </tbody>
        </template>
      </v-simple-table>
    </v-container>
  </template>
  
  <script>
  import { mapActions, mapGetters } from "vuex";
  export default {
    name: "songList",
    async mounted() {
      await this.getsongsAction();
      this.songList = this.songs.map((pl) => pl);
    },
    data() {
      return {
        songList: [],
        loading: false,
      };
    },
    methods: {
      ...mapActions("songModule", ["getsongsAction"]),
      getLang(language) {
        switch (language) {
          case 0:
            return "İngilizce";
          case 1:
            return "Türkçe";
          case 2:
            return "İspanyolca";
          default:
            return "Bilemedim";
        }
      },
    },
    computed: {
      ...mapGetters("songModule", {
        songs: "songs",
      }),
    },
  };
  </script>