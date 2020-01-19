(function (Sys) {

    'use strict';
    Sys.Application.add_load(function () {

        // Loop through each of the limited option fields
        $('.registrationentry-registrant [data-attribute-id]').each(function (index) {

            // Save the parent element
            var $parent = $(this);

            // Get the registration instance and attribute ids
            var existingValue = $parent.data('existing-value');
            var instanceId = $parent.data('instance-id');
            var attributeId = $parent.data('attribute-id');

            // Set the REST Url
            var url = Rock.settings.get('baseUrl') + 'api/LimitedOptionsFieldType/AvailableQuantities/' + instanceId + '/' + attributeId;

            // Get the number of available items for each option
            $.get(url, function (items) {

                // Loop through each option
                $.each(items, function (key, value) {

                    // If there are not any items remaining
                    if (value <= 0 && ( existingValue == null || existingValue != key ) ) {

                        // Find the ListItem
                        $parent.find("[value = '" + key + "']").each(function () {

                            // Save the ListItem
                            var $elem = $(this);

                            // Check to see if the attribute was configured to show text indicating item is 'full'
                            if ($elem.data('full-text') == '') {

                                // If not...
                                if ($elem.is('input')) {
                                    //... and item is a radio button, hide the parent element (div)
                                    $elem.parent().hide();
                                } else {
                                    //... and item is a select, hide the option
                                    $elem.hide();
                                }
                            } else {

                                // If so...
                                // Disable the option
                                $elem.attr('disabled', true);

                                if ($elem.is('input')) {
                                    //... and item is a radio buttion, update the following label to be diabled, muted, and include the text
                                    $elem.next().attr('disabled', true);
                                    $elem.next().addClass('text-muted');
                                    $elem.next().html($elem.data('full-text'));
                                } else {
                                    //... and item is a select, update the option to include the text
                                    $elem.addClass('text-muted');
                                    $elem.html($elem.data('full-text'));
                                }
                            }
                        });
                    }
                });
            });
        });
    });
}(Sys));
