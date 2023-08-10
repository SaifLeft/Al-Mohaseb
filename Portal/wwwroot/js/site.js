let styleSwitcherToggleEl = document.querySelector('.style-switcher-toggle');
if (window.templateCustomizer) {
    // setStyle light/dark on click of styleSwitcherToggleEl
    if (styleSwitcherToggleEl) {
        styleSwitcherToggleEl.addEventListener('click', function () {
            if (window.Helpers.isLightStyle()) {
                window.templateCustomizer.setStyle('dark');
            } else {
                window.templateCustomizer.setStyle('light');
            }
        });
    }
    // Update style switcher icon and tooltip based on current style
    if (window.Helpers.isLightStyle()) {
        if (styleSwitcherToggleEl) {
            styleSwitcherToggleEl.querySelector('i').classList.add('bx-moon');
            new bootstrap.Tooltip(styleSwitcherToggleEl, {
                title: 'Dark mode',
                fallbackPlacements: ['bottom']
            });
        }
        switchImage('light');
    } else {
        if (styleSwitcherToggleEl) {
            styleSwitcherToggleEl.querySelector('i').classList.add('bx-sun');
            new bootstrap.Tooltip(styleSwitcherToggleEl, {
                title: 'Light mode',
                fallbackPlacements: ['bottom']
            });
        }
        switchImage('dark');
    }
}

// Update light/dark image based on current style
function switchImage(style) {
    const switchImagesList = [].slice.call(document.querySelectorAll('[data-app-' + style + '-img]'));
    switchImagesList.map(function (imageEl) {
        const setImage = imageEl.getAttribute('data-app-' + style + '-img');
        imageEl.src = assetsPath + 'img/' + setImage; // Using window.assetsPath to get the exact relative path
    });
}