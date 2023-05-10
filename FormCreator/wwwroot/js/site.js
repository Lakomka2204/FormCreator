// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function copyText(callerId, elementId) {
    var element = $('#' + elementId);
    var text = element.text();
    navigator.clipboard.writeText(text);
    if (window.getSelection) { window.getSelection().removeAllRanges(); }
    else if (document.selection) { document.selection.empty(); }
    element.blur();
    changeClass();
}
let timeoutId;
function changeClass() {
    if (timeoutId) return;
    var iconEl = document.getElementById("copyIcon");
    iconEl.classList.remove('bi-clipboard2');
    iconEl.classList.remove('text-primary');
    iconEl.classList.add("text-success");
    iconEl.classList.add("bi-clipboard2-check-fill");

    timeoutId = setTimeout(function () {
        iconEl.classList.remove('bi-clipboard2-check-fill');
        iconEl.classList.remove("text-success");
        iconEl.classList.add('text-primary');
        iconEl.classList.add('bi-clipboard2');
        timeoutId = undefined;
    }, 3000);
}
var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
})