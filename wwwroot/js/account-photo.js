(() => {
    const input = document.getElementById('Photo');
    if (!input) return;
    const preview = document.getElementById('photo-preview');
    const feedback = document.getElementById('photo-feedback');
    const original = preview.innerHTML;
    let previewUrl;
    input.addEventListener('change', () => {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        feedback.textContent = '';
        preview.innerHTML = original;
        const file = input.files[0];
        if (!file) return;
        if (!['image/jpeg', 'image/png'].includes(file.type) || file.size > 512 * 1024) {
            feedback.textContent = 'Choose a JPG or PNG photo no larger than 512 KB.';
            input.value = '';
            return;
        }
        previewUrl = URL.createObjectURL(file);
        const image = document.createElement('img');
        image.alt = 'Preview of your selected photo';
        image.src = previewUrl;
        image.onerror = () => {
            preview.innerHTML = original;
            input.value = '';
            feedback.textContent = 'This photo could not be opened. Choose another image.';
        };
        preview.replaceChildren(image);
        feedback.textContent = 'Photo selected. Save changes to upload it.';
    });
})();
