// firebase-init.js
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.23.0/firebase-app.js";
import { getAnalytics } from "https://www.gstatic.com/firebasejs/9.23.0/firebase-analytics.js";
import { getDatabase } from "https://www.gstatic.com/firebasejs/9.23.0/firebase-database.js";
import { getAuth } from "https://www.gstatic.com/firebasejs/9.23.0/firebase-auth.js";

const firebaseConfig = {
    apiKey: "AIzaSyBEjxaEf_S9x9NZhmJrWwHLDdZvoyDajDg",
    authDomain: "alabaster-8cfcd.firebaseapp.com",
    databaseURL: "https://alabaster-8cfcd-default-rtdb.firebaseio.com",
    projectId: "alabaster-8cfcd",
    storageBucket: "alabaster-8cfcd.appspot.com",
    messagingSenderId: "1014314025942",
    appId: "1:1014314025942:web:fedff22641b2b8bd219a77",
    measurementId: "G-27PZK6GB89"
};

const app = initializeApp(firebaseConfig);
const analytics = getAnalytics(app);
const database = getDatabase(app);
const auth = getAuth(app);

export { app, analytics, database, auth };
