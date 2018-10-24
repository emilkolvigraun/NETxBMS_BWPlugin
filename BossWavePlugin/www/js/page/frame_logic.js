function adjust_height(element) {
    element.style.height = element.scrollHeight + 'px';
    document.body.height = element.style.height + '100px';
}