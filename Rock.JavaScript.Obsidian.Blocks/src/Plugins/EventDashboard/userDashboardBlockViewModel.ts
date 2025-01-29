import { ContentChannelItemBag } from "../ViewModels/contentChannelItemBag"
import { ContentChannelItemAssociationBag } from "../ViewModels/contentChannelItemAssociationBag"
import { DefinedValueBag } from "../ViewModels/definedValueBag"
import { AttributeBag } from "../ViewModels/attributeBag"

export type UserDashboardBlockViewModel = {
    events: ContentChannelItemBag[];
    eventDetails: ContentChannelItemAssociationBag[];
    comments: ContentChannelItemBag[];
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    locations: DefinedValueBag[];
    ministries: DefinedValueBag[];
    budgetLines: DefinedValueBag[];
    drinks: DefinedValueBag[];
    requestStatus: AttributeBag;
    requestType: AttributeBag;
    workflowURL: string;
    defaultStatuses: string[];
    eventDetailsCCId: number;
    commentsCCId: number;
};

export type DuplicateRequestViewModel = {
    request: ContentChannelItemBag;
    events: ContentChannelItemBag[];
}