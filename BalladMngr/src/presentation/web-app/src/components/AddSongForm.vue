<template>
  <v-row justify="center">
    <v-dialog v-model="dialog" persistent max-width="600px">
      <template v-slot:activator="{ on, attrs }">
        <v-btn
          style="margin-top: 1rem"
          rounded
          color="light-blue"
          dark
          v-bind="attrs"
          v-on="on"
        >
          <v-icon left>mdi-plus</v-icon>
          Koleksiyona yeni bir beste ekle.
        </v-btn>
      </template>
      <v-card>
        <form
          @submit.prevent="
            addSongAction(body);
            body = {};
          "
        >
          <v-card-title>
            <span class="headline">Şarkı Bilgileri</span>
          </v-card-title>
          <v-card-text>
            <v-container>
              <v-row>
                <v-col cols="12" sm="6">
                  <v-text-field
                    label="Adı"
                    v-model="body.title"
                    @input="$v.body.title.$touch()"
                    @blur="$v.body.title.$touch()"
                    :error-messages="titleErrors"
                    required
                  >
                  </v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-textarea
                    label="Sözleri"
                    v-model="body.lyrics"
                    @input="$v.body.lyrics.$touch()"
                    @blur="$v.body.lyrics.$touch()"
                    :error-messages="lyricErrors"
                    required
                  >
                  </v-textarea>
                </v-col>

                <v-col cols="12" sm="6">
                  <v-select
                    v-model="body.language"
                    :items="languages"
                    item-text="name"
                    item-value="id"
                    label="Dili"
                    persistent-hint
                    return-object
                    single-line
                  >
                  </v-select>
                </v-col>
              </v-row>
            </v-container>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="dialog = false">
              Close
            </v-btn>
            <v-btn
              color="blue darken-1"
              text
              @click="dialog = false"
              :disabled="$v.$anyError"
              type="submit"
            >
              Save
            </v-btn>
          </v-card-actions>
        </form>
      </v-card>
    </v-dialog>
  </v-row>
</template>

<script>
import { mapActions } from "vuex";
import validators from "@/validators";
export default {
  name: "AddSongForm",
  data: () => ({
    body: {
      title: "",
      lyrics: "",
      language: 1,
    },
    dialog: false,
    languages: [
      { id: 0, name: "İngilizce" },
      { id: 1, name: "Türkçe" },
      { id: 2, name: "İspanyolca" },
    ],
  }),
  methods: {
    ...mapActions("SongModule", ["addSongAction"]),
  },
  computed: {
    titleErrors() {
      const errors = [];
      if (!this.$v.body.title.$dirty) return errors;
      !this.$v.body.title.required && errors.push("Lütfen şarkının adını yaz.");
      !this.$v.body.title.maxLength && errors.push("En fazla 50 karakter.");
      return errors;
    },
    lyricErrors() {
      const errors = [];
      if (!this.$v.body.lyrics.$dirty) return errors;
      !this.$v.body.lyrics.required &&
        errors.push("Lütfen şarkı sözlerini gir.");
      !this.$v.body.lyrics.minLength && errors.push("En az 50 karakter olsun");
      !this.$v.body.lyrics.maxLength && errors.push("En fazla 1000 karakter.");
      return errors;
    },
  },
  validations: {
    body: {
      title: validators.addSong.title,
      lyrics: validators.addSong.lyrics,
    },
  },
};
</script>
