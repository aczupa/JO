(function () {
    // inicjalizator — podłącza eventy do linków
    function initVideoNavScroll() {
        const links = document.querySelectorAll('.video-nav-bar a[href^="#"]');
        if (!links.length) return;

        function getHeaderOffset() {
            const nav = document.querySelector('nav.navbar');
            return nav ? Math.ceil(nav.getBoundingClientRect().height) + 12 : 12;
        }

        function smoothScrollToElement(el) {
            if (!el) return;
            const rect = el.getBoundingClientRect();
            const offset = getHeaderOffset();
            const top = window.pageYOffset + rect.top - offset;
            window.scrollTo({
                top: Math.max(0, Math.floor(top)),
                behavior: 'smooth'
            });
        }

        links.forEach(link => {
            // 🔥 usuń stary handler, jeśli istnieje
            link.onclick = null;

            link.addEventListener('click', function (ev) {
                const href = this.getAttribute('href');
                if (!href || href === '#') return;

                const id = href.slice(1);
                const target = document.getElementById(id);
                if (!target) return;

                ev.preventDefault();

                // scroll
                smoothScrollToElement(target);

                // update hash bez skoku
                history.replaceState(null, '', `#${id}`);

                // zamknij mobilny collapse
                const bsCollapse = document.querySelector('.navbar-collapse.show');
                if (bsCollapse && window.bootstrap?.Collapse) {
                    bootstrap.Collapse.getOrCreateInstance(bsCollapse).hide();
                }

                // highlight
                target.classList.add('scroll-target-highlight');
                setTimeout(() => target.classList.remove('scroll-target-highlight'), 1200);
            });
        });

        // prefer CSS smooth jeśli wspierane
        try { document.documentElement.style.scrollBehavior = 'smooth'; } catch (e) { }
    }

    // expose do Blazor
    window.initVideoNavScroll = initVideoNavScroll;

    // scroll to top helper
    window.scrollToTop = () => window.scrollTo(0, 0);

    // 🔹 nowa funkcja do scroll po hashie (po renderze)
    window.scrollToHashIfAny = () => {
        const hash = window.location.hash;
        if (!hash) return;

        const id = hash.substring(1);
        const target = document.getElementById(id);
        if (!target) return;

        const nav = document.querySelector('nav.navbar');
        const offset = nav ? Math.ceil(nav.getBoundingClientRect().height) + 12 : 12;

        const rect = target.getBoundingClientRect();
        const top = window.pageYOffset + rect.top - offset;

        window.scrollTo({
            top: Math.max(0, Math.floor(top)),
            behavior: 'smooth'
        });
    };
})();


document.addEventListener("DOMContentLoaded", () => {

    const grid = document.querySelector(".antarctica-grid");

    grid.addEventListener("click", (e) => {

        const item = e.target.closest(".accordion-item");
        if (!item) return;

        // zamknij inne
        document.querySelectorAll(".accordion-item.active").forEach(el => {
            if (el !== item) el.classList.remove("active");
        });

        // toggle klikniętego
        item.classList.toggle("active");
    });
});

(function () {
    window.initPolynesieOverlay = function () {
        const cells = document.querySelectorAll(".cell");
        const overlay = document.getElementById("overlay");
        const overlayText = document.getElementById("overlay-text");

        if (!cells.length || !overlay || !overlayText) return;

        const descriptions = [
            `
            ✨ <strong>Jour 1 — Tahiti : L’aube du voyage</strong><br><br>
            Accueil chaleureux avec colliers de fleurs.<br>
            Installation à bord et cocktail au coucher du soleil.<br>
            Dîner avec spectacle de danse tahitienne traditionnelle (<em>ʻOri Tahiti</em>).
            `,
            `
            🏝️ <strong>Jour 2 — Moorea : L’île magique</strong><br><br>
            Navigation au lever du soleil, face aux majestueux pics volcaniques.<br>
            <strong>Excursion :</strong><br>
            – Tour du lagon en pirogue polynésienne<br>
            – Nage avec raies et requins pointe noire<br>
            Après-midi : détente sur plage privée et dégustation de jus de fruits locaux.<br>
            Soirée : cinéma en plein air sous les étoiles.
            `,
            `
            🌊 <strong>Jour 3 — Moorea : Nature et traditions</strong><br><br>
            Randonnée légère au Belvédère pour admirer la vue panoramique.<br>
            Atelier de monoi et tressage de feuilles de pandanus.<br>
            Dîner gastronomique avec produits locaux : mahi-mahi, vanille, taro.
            `,
            `
            🏖️ <strong>Jour 4 — Huahine : L’île sauvage</strong><br><br>
            Matin : visite des marae, anciens temples polynésiens.<br>
            Après-midi : découverte des fermes perlières.<br>
            Apéritif sur le pont accompagné de musique traditionnelle à l’ukulélé.
            `,
            `
            🌺 <strong>Jour 5 — Raiatea : Le cœur sacré de la Polynésie</strong><br><br>
            Découverte du marae Taputapuatea (site UNESCO).<br>
            Sortie en kayak sur la seule rivière navigable de Polynésie.<br>
            Cours de danse polynésienne à bord.
            `,
            `
            💎 <strong>Jour 6 — Tahaa : L’île vanille</strong><br><br>
            Visite d’une plantation de vanille.<br>
            Snorkeling dans un jardin de corail coloré.<br>
            Dîner sur un motu privé éclairé par des torches polynésiennes.
            `,
            `
            🏝️ <strong>Jour 7 — Bora Bora : Le lagon des lagons</strong><br><br>
            Entrée dans le lagon turquoise — un moment magique.<br>
            Tour du lagon avec arrêt pour baignade dans les eaux translucides.<br>
            Option : survol en hélicoptère du mont Otemanu.<br>
            Dîner romantique sur le pont.
            `,
            `
            🌅 <strong>Jour 8 — Bora Bora : Journée détente</strong><br><br>
            Journée libre pour profiter de :<br>
            – Spa polynésien<br>
            – Paddle, kayak, snorkeling<br>
            – Plage sur un motu privé<br>
            Soirée : concert d’ukulélé et feu de joie.
            `,
            `
            🌊 <strong>Jour 9 — Rangiroa : Le royaume des plongeurs</strong><br><br>
            Exploration du lagon immense.<br>
            Plongée ou snorkeling dans la passe de Tiputa.<br>
            Dégustation des vins locaux du vignoble de Rangiroa.
            `,
            `
            🌞 <strong>Jour 10 — Retour à Tahiti</strong><br><br>
            Dernier lever de soleil sur le Pacifique.<br>
            Débarquement en douceur au rythme des tambours.<br>
            Souvenirs : colliers de fleurs séchées, vanille et perles.
            `
            
        ];

        cells.forEach((cell, index) => {
            cell.addEventListener("click", () => {
                overlayText.innerHTML = descriptions[index] ?? "Programme à venir…";
                overlay.classList.add("active");
            });
        });

        window.closeOverlay = () => overlay.classList.remove("active");
    }
})();

(function () {

    window.initAlaskaOverlay = function () {

        const cells = document.querySelectorAll(".cell");
        const overlay = document.getElementById("overlay");
        const overlayText = document.getElementById("overlay-text");

        if (!cells.length || !overlay || !overlayText) return;

        const descriptions = [

            `
            <strong>Jour 1 – Embarquement & départ</strong><br><br>
            Accueil au port et embarquement à bord du navire.<br>
            Installation en cabine, premières découvertes du bateau.<br><br>
            Le voyage commence, cap vers les terres sauvages de l’Alaska.
            `,

            `
            <strong>Jour 2 – Navigation en mer</strong><br><br>
            Journée de navigation le long du Pacifique Nord.<br>
            Conférences, observation des oiseaux marins et détente à bord.<br><br>
            L’horizon s’élargit, le rythme ralentit.
            `,

            `
            <strong>Jour 3 – Fjords & Inside Passage</strong><br><br>
            Entrée dans l’Inside Passage, l’une des plus belles routes maritimes du monde.<br>
            Montagnes abruptes, forêts profondes et eaux calmes.<br><br>
            Une navigation spectaculaire et silencieuse.
            `,

            `
            <strong>Jour 4 – Glaciers majestueux</strong><br><br>
            Approche des grands glaciers d’Alaska.<br>
            La glace millénaire se fracture et glisse lentement vers la mer.<br><br>
            Un moment de pure contemplation.
            `,

            `
            <strong>Jour 5 – Faune sauvage</strong><br><br>
            Observation des baleines, orques, phoques et aigles.<br>
            Chaque apparition est un privilège rare et fugace.<br><br>
            La nature règne en maître.
            `,

            `
            <strong>Jour 6 – Escale en Alaska</strong><br><br>
            Escale dans une ville emblématique (Juneau, Skagway ou Ketchikan).<br>
            Découverte de l’histoire locale et des cultures autochtones.<br><br>
            Entre civilisation et immensité sauvage.
            `,

            `
            <strong>Jour 7 – Forêts boréales & montagnes</strong><br><br>
            Navigation le long de vastes étendues forestières.<br>
            Brume légère, sommets enneigés, silence profond.<br><br>
            Le temps semble suspendu.
            `,

            `
            <strong>Jour 8 – Navigation polaire</strong><br><br>
            Le navire progresse entre glace, mer et ciel changeant.<br>
            Observation des paysages depuis le pont ou le salon panoramique.<br><br>
            Une expérience hors du monde.
            `,

            `
            <strong>Jour 9 – Ciel du Nord & derniers glaciers</strong><br><br>
            Derniers panoramas glacés avant le retour.<br>
            Selon les conditions, observation du ciel boréal.<br><br>
            Un final tout en émotion.
            `,

            `
            <strong>Jour 10 – Retour & débarquement</strong><br><br>
            Arrivée au port et débarquement.<br>
            Fin de la croisière après un voyage hors du temps.<br><br>
            L’Alaska laisse une empreinte durable.
            `
        ];

        cells.forEach((cell, index) => {
            cell.addEventListener("click", () => {
                overlayText.innerHTML = descriptions[index % descriptions.length];
                overlay.classList.add("active");
            });
        });
    };

    window.closeOverlay = function () {
        document.getElementById("overlay")?.classList.remove("active");
    };

})();

    (function () {

        let nilFilmInterval1 = null;
    let nilFilmInterval2 = null;

    function preload(frames) {
        return frames.map(src => {
            const img = new Image();
    img.src = src;
    return img;
        });
    }

    // ===== FILM 1 =====
    window.startNilFilm = function () {
        if (nilFilmInterval1 !== null) return;

    const frames = preload([
    "/images/nil/nile1.jpg",
    "/images/nil/luxor1.jpg",
    "/images/nil/nile2.jpg",
    "/images/nil/nile3.jpg",
    "/images/nil/nile4.jpg",
    "/images/nil/nile11.jpg",
    "/images/nil/nile5.jpg",
    "/images/nil/nile6.jpg",
    "/images/nil/nile7.jpg",
    "/images/nil/nile8.jpg",
    "/images/nil/nile9.jpg",
    "/images/nil/nile10.jpg"
    ]);

    let i = 0;

        nilFilmInterval1 = setInterval(() => {
            const img = document.getElementById("nilFrame");
    if (!img) return;

    i = (i + 1) % frames.length;
    img.src = frames[i].src;
        }, 1000);
    };

    window.stopNilFilm = function () {
        clearInterval(nilFilmInterval1);
    nilFilmInterval1 = null;
    };

    // ===== FILM 2 =====
    window.startNilFilm2 = function () {
        if (nilFilmInterval2 !== null) return;

    const frames = preload([
    "/images/nil/egypt1.jpg",
    "/images/nil/egypt13.jpg",
    "/images/nil/egypt2.jpg",
    "/images/nil/egypt3.jpg",
    "/images/nil/egypt4.jpg",
    "/images/nil/egypt5.jpg",
    "/images/nil/egypt12.jpg",
    "/images/nil/egypt6.jpg",
    "/images/nil/egypt7.jpg",
    "/images/nil/egypt8.jpg",
    "/images/nil/egypt9.jpg",
    "/images/nil/egypt10.jpg",
    "/images/nil/egypt11.jpg"
    ]);

    let i = 0;

        nilFilmInterval2 = setInterval(() => {
            const img = document.getElementById("nilFrame2");
    if (!img) return;

    i = (i + 1) % frames.length;
    img.src = frames[i].src;
        }, 1000);
    };

    window.stopNilFilm2 = function () {
        clearInterval(nilFilmInterval2);
    nilFilmInterval2 = null;
    };

})();


window.stopPropagation = function (e) {
    if (e) e.stopPropagation();
};




