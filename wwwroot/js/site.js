// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// =========================================
// IMAGE PREVIEW - Costumes Create
// =========================================
function previewCreateImage(input) {
    const preview = document.getElementById('createPreview');
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// =========================================
// IMAGE PREVIEW - Costumes Edit
// =========================================
function handleImagePreview(input, imgElementId, labelId) {
    const previewImage = document.getElementById(imgElementId);
    const label = document.getElementById(labelId);

    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            previewImage.src = e.target.result;
            if (label) {
                label.innerText = 'Нова снимка избрана:';
                label.style.color = '#ff7518';
            }
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// =========================================
// BOOTSTRAP TOOLTIPS - global init
// =========================================
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
