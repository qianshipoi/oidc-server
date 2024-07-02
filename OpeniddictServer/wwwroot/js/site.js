const themeToggle = document.querySelector("#switch-theme");

themeToggle.addEventListener("click", () => {
    document.body.classList.contains("light-theme")
        ? enableDarkMode()
        : enableLightMode();
});

function enableDarkMode() {
    document.body.classList.remove("light-theme");
    document.body.classList.add("dark-theme");
    themeToggle.setAttribute("aria-label", "Switch to light theme");
    document.documentElement.setAttribute('data-bs-theme', "dark");
    localStorage.setItem("color-scheme", "dark");
}

function enableLightMode() {
    document.body.classList.remove("dark-theme");
    document.body.classList.add("light-theme");
    themeToggle.setAttribute("aria-label", "Switch to dark theme");
    document.documentElement.removeAttribute('data-bs-theme');
    localStorage.setItem("color-scheme", "light");
}

function setThemePreference() {
    const colorScheme = localStorage.getItem("color-scheme");
    if (colorScheme) {
        colorScheme === "dark" ? enableDarkMode() : enableLightMode();
        return;
    } 

    if (window.matchMedia("(prefers-color-scheme: dark)").matches) {
        enableDarkMode();
        return;
    }
    enableLightMode();
}

document.onload = setThemePreference();