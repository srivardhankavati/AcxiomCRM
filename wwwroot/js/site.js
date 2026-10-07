// Small client-side helpers used by the CRM pages.
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-confirm]').forEach(button => {
        button.addEventListener('click', event => {
            const message = button.getAttribute('data-confirm') || 'Continue with this action?';
            if (!window.confirm(message)) event.preventDefault();
        });
    });
});
