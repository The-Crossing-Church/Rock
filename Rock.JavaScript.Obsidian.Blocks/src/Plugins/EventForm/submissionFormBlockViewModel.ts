import { DefinedValueBag } from "@Obsidian/ViewModels/Entities/definedValueBag"
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { ContentChannelItemAssociationBag } from "@Obsidian/ViewModels/Entities/contentChannelItemAssociationBag"
import { AttributeMatrixBag } from "@Obsidian/ViewModels/Entities/attributeMatrixBag"
import { AttributeMatrixItemBag } from "@Obsidian/ViewModels/Entities/attributeMatrixItemBag"
import { AttributeBag } from "@Obsidian/ViewModels/Entities/attributeBag"

export type SubmissionFormBlockViewModel = {
    request: ContentChannelItemBag;
    originalRequest: ContentChannelItemBag;
    events: ContentChannelItemBag[];
    existing: ContentChannelItemBag[];
    existingDetails: ContentChannelItemAssociationBag[];
    isSuperUser: boolean;
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    permissions: string[];
    locations: DefinedValueBag[];
    locationSetupMatrix: AttributeMatrixBag[];
    locationSetupMatrixItem: AttributeMatrixItemBag[];
    ministries: DefinedValueBag[];
    budgetLines: DefinedValueBag[];
    inventoryList: DefinedValueBag[];
    adminDashboardURL: string;
    userDashboardURL: string;
    discountCodeAttrs: AttributeBag[];
};