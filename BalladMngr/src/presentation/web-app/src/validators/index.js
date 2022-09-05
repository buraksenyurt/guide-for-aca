import { required, minLength, maxLength } from "vuelidate/lib/validators";

export default {
  addSong: {
    title: {
      required,
      maxLength: maxLength(50),
    },
    lyrics: {
      required,
      minLength: minLength(50),
      maxLength: maxLength(1000),
    },
  },
};
