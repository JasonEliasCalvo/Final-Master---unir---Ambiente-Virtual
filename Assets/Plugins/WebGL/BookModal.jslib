mergeInto(LibraryManager.library, {
    OpenBookModal: function (urlPtr) {
        const url = UTF8ToString(urlPtr);
        if (typeof window.OpenBookModal === 'function') {
            window.OpenBookModal(url);
        } else {
            console.error("window.OpenBookModal no está definido");
        }
    },

    CloseBookModal: function () {
        if (typeof window.CloseBookModal === 'function') {
            window.CloseBookModal();
        } else {
            console.error("window.CloseBookModal no está definido");
        }
    }
});
