(function (Sys) {

    'use strict';
    Sys.Application.add_load(function () {

        var $parent = $('div.registration-additional-options');

        // Find the first fee element (it contains a fee id that the REST method can then use to extrapolate all the fee options)
        var $fees = $parent.find('.rock-drop-down-list select[id*="_fee_"]');
        if ($fees.length) {

            var nameParts = $fees.first().attr('name').split('_');
            if (nameParts && nameParts.length > 0) {

                // Get the fee id
                var feeId = nameParts[nameParts.length - 1];

                // Set the REST Url
                var url = Rock.settings.get('baseUrl') + 'api/LimitedRegistrationFeeOptions/UnavailableOptions/' + feeId;

                // Get the options that are unavailable
                $.get(url, function (items) {

                    // Loop through each option
                    $.each(items, function (index, item) {

                        // Find the fee
                        var $select = $parent.find('select[name$="fee_' + item.FeeId + '"]');

                        // Find the value
                        var $value = $select.find("[value = '" + item.Option + "']");

                        // Disable the value
                        $value.attr('disabled', true);
                        $value.addClass('text-muted');
                        $value.html($value.html() + ' (FULL)');

                    });
                });
            }
        }
    });
}(Sys));
