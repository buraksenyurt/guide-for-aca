import Vue from "vue";
import VueRouter from "vue-router";
import Home from "@/views/HomeView.vue";
import SongList from "@/views/dashboard/SongList";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "Home",
    component: Home,
  },
  {
    path: "/about",
    name: "About",
    component: () =>
      import(/* webpackChunkName: "about" */ "../views/AboutView.vue"),
    meta: {
      requiresAuth: false, // Hakkında sayfası için authorization gerekli değil.
    },
  },
  {
    path: "/dashboard",
    component: () => import("@/views/dashboard"),
    meta: {
      requiresAuth: false
    },
    children: [
      {
        path: "",
        component: () => import("@/views/dashboard/DefaultContent"),
      },
      {
        path: "song-list",
        component: SongList,
      }
    ],
  }    
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes,
});

export default router;