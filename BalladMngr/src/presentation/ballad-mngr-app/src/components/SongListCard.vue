<template>

    <v-skeleton-loader v-if="loading"
                       width="500"
                       max-width="600"
                       height="100%"
                       type="card"></v-skeleton-loader>

    <v-card v-else width="500" max-width="600" height="100%">
        <v-toolbar color="pink" dark>
            <v-toolbar-title>Besteler</v-toolbar-title>
            <v-spacer></v-spacer>
        </v-toolbar>
        <v-list-item-group color="primary">
            <v-list-item v-for="song in songs" :key="song.id">
                <v-list-item-content>
                    <v-list-item-title v-text="song.title"></v-list-item-title>
                    <v-list-item-subtitle v-text="song.status"></v-list-item-subtitle>
                </v-list-item-content>
                <v-list-item-action>
                    <v-icon @click="removeSong(song.id)">
                        mdi-delete-outline
                    </v-icon>
                </v-list-item-action>
            </v-list-item>
        </v-list-item-group>
    </v-card>
</template>


<script>
    import { mapActions, mapGetters } from "vuex";
    export default {
        name: "SongListCard",
        computed: {
            ...mapGetters("songModule", {
                books: "songs",
                loading: "loading",
            }),
        },
        methods: {
            ...mapActions("songModule", ["removeSongAction"]),
            removeSong(songId) {
                const confirmed = confirm(
                    "Bu besteyi silmek istediğine emin misin?"
                );
                if (!confirmed) return;
                this.removeSongAction(bookId);
            },
        },
    };
</script>