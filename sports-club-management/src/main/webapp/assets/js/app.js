// Shared client-side behaviors.
// CSP forbids inline scripts/handlers (script-src 'self'), so confirmation
// prompts are wired here via data attributes instead of inline onclick.
//
// Usage: add data-confirm="Your message" to any <form>. Submitting it
// (button click or Enter) shows a confirm() dialog and cancels on "Cancel".
document.addEventListener('submit', function (e) {
    var form = e.target;
    if (form && form.matches && form.matches('form[data-confirm]')) {
        if (!window.confirm(form.getAttribute('data-confirm'))) {
            e.preventDefault();
        }
    }
});

// Apply dynamic widths from data-width (e.g. progress bars). Setting the
// width via the CSSOM here keeps it out of an inline style="" attribute,
// which the CSP style-src ('self', no 'unsafe-inline') would otherwise block.
document.addEventListener('DOMContentLoaded', function () {
    var bars = document.querySelectorAll('[data-width]');
    for (var i = 0; i < bars.length; i++) {
        bars[i].style.width = bars[i].getAttribute('data-width') + '%';
    }
});
