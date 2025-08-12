// Toast notification functions for Assessment Batch
window.showSuccessToast = (message) => {
    // Using simple alert for now, can be replaced with proper toast library
    alert("✅ " + message);
};

window.showErrorToast = (message) => {
    alert("❌ " + message);
};

window.showInfoToast = (message) => {
    alert("ℹ️ " + message);
};

window.showWarningToast = (message) => {
    alert("⚠️ " + message);
};

// Confirmation dialog
window.confirm = (message) => {
    return confirm(message);
};
