// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function copyText(callerId, elementId) {
    if (!navigator.clipboard) return console.log("HTTPS PROTOCOL REQUIRED");
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
function dynamicPage(url, changeurl) {
    $.ajax({
        url: url,
        method: 'GET',
        dataType: 'html',
        success: function (data) {
            if (changeurl) {
                window.history.replaceState({}, '', url);
            }
            document.open();
            document.write(data);
            document.close();
            var pageLoadedEvent = new CustomEvent('pageLoaded');
            window.dispatchEvent(pageLoadedEvent);
        },
        error: function (xhr, status, error) {
            setTimeout(function () {
                var myModalEl = document.getElementById('serviceUnavailable');
                var modal = new window.bootstrap.Modal(myModalEl);
                modal.show();
            }, 100);
        }
    });
}
