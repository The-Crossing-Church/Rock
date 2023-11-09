import { DefinedValueBag } from "@Obsidian/ViewModels/Entities/definedValueBag"
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { ContentChannelItemAssociationBag } from "@Obsidian/ViewModels/Entities/contentChannelItemAssociationBag"
import { AttributeBag } from "@Obsidian/ViewModels/Entities/attributeBag"

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