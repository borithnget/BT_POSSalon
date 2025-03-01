$(document).ready(function () {
    $('#add').click(function () {
        var isAllValid = true;
        var $newrow = $('#mainrow').clone().removeAttr('id');
        $('.pc', $newrow).val($('#').val());
        $('.pc', $newrow).val($('#').val());

        $('#add', $newrow).addClass('remove').val('Remove').removeClass('btn-success').addClass('btn-danger')

    })
})