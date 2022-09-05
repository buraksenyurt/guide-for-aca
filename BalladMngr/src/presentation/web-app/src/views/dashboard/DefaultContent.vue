<template>
    <div>
        <div class="text-h2 my-4">Besteler Panosu</div>
        <div class="default-content">
            <div style="margin-right: 4rem; margin-bottom: 4rem">
                <SongListCard @handleShowBooks="handleShowSongs" />
                <AddSongForm />
            </div>
        </div>
        <div v-if="showSongs">
            <AddSongForm :songId="songId" />
        </div>
    </div>
</template>

<script>
    import { mapActions } from "vuex";
    import SongListCard from "@/components/SongListCard";
    import AddSongForm from "@/components/AddSongForm";
    export default {
        name: "DefaultContent",
        components: {
            SongListCard,
            AddSongForm
        },
        methods: {
            ...mapActions("songModule", ["getSongsAction"]),
            handleShowSongs(show, id) {
                this.showSongs = show;
                this.songId = id;
            },
        },
        data: () => ({
            showSongs: false,
            bookId: 0
        }),
        mounted() {
            this.getSongsAction();
            this.showSongs = false;
        },
    };
</script>


<style scoped>
    .default-content {
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: flex-start;
    }
</style>