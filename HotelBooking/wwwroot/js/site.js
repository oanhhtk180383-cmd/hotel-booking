document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => {
    if (!confirm(form.dataset.confirm)) event.preventDefault();
}));
document.querySelectorAll('.flash-message').forEach(message => window.setTimeout(() => {
    if (window.bootstrap?.Alert) bootstrap.Alert.getOrCreateInstance(message).close();
    else message.remove();
}, 3000));
const header = document.querySelector('.header-overlay');
if (header) {
    const updateHeader = () => header.classList.toggle('scrolled', window.scrollY > 45);
    window.addEventListener('scroll', updateHeader, { passive: true });
    updateHeader();
}
if (window.jQuery) $(function () {
if ($.validator) {
    $.validator.addMethod('hotelArrival', function(value, element) {
        return this.optional(element) || (/^\d{4}-\d{2}-\d{2}$/.test(value) && value >= element.min);
    }, 'Ngày nhận phòng phải từ hôm nay.');
    $.validator.addMethod('hotelDeparture', function(value, element) {
        const start = element.form.querySelector('[name="NgayNhan"]');
        return this.optional(element) || (/^\d{4}-\d{2}-\d{2}$/.test(value) && value > start.value);
    }, 'Ngày trả phòng phải sau ngày nhận phòng.');
    $.validator.messages.number = 'Vui lòng nhập số hợp lệ.';
    $.validator.messages.date = 'Ngày không hợp lệ.';
    $.validator.messages.digits = 'Vui lòng chỉ nhập chữ số.';
    $.validator.messages.min = $.validator.format('Giá trị phải từ {0}.');
    $.validator.messages.max = $.validator.format('Giá trị không được vượt quá {0}.');
    document.querySelectorAll('form').forEach(form => {
        const start = form.querySelector('[name="NgayNhan"]');
        const end = form.querySelector('[name="NgayTra"]');
        if (start?.type === 'date' && end?.type === 'date') {
            $(start).rules('add', { hotelArrival: true, messages: { min: 'Ngày nhận phòng phải từ hôm nay.' } });
            $(end).rules('add', { hotelDeparture: true, messages: { min: 'Ngày trả phòng phải sau ngày nhận phòng.' } });
            const update = () => {
                if (!start.value) return;
                const day = new Date(start.value + 'T00:00:00Z');
                if (Number.isNaN(day.getTime())) return;
                day.setUTCDate(day.getUTCDate() + 1);
                end.min = day.toISOString().slice(0, 10);
                if ($(end).hasClass('input-validation-error')) $(end).valid();
            };
            start.addEventListener('change', update);
            update();
        }
    });
}
});
