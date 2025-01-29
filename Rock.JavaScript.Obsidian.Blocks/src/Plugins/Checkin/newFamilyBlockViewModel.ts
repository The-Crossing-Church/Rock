import { DefinedValueBag } from "../ViewModels/definedValueBag"
import { DefinedTypeBag } from "../ViewModels/definedTypeBag"
import { AttributeBag } from "../ViewModels/attributeBag"
import { PersonBag } from "../ViewModels/personBag"
import { PhoneNumberBag } from "../ViewModels/phoneNumberBag"
import { GroupBag } from "../ViewModels/groupBag"
export type NewFamilyBlockViewModel = {
  showTitle: string;
  titleDefinedType: DefinedTypeBag;
  showNickName: string;
  showMiddleName: string;
  showSuffix: string;
  suffixDefinedType: DefinedTypeBag;
  connectionStatusDefinedType: DefinedTypeBag;
  defaultConnectionStatus: DefinedValueBag;
  requireConnectionStatus: String;
  requireGender: string;
  requireBirthDate: string;
  requireGradeOrAbility: string;
  showMaritalStatus: string;
  maritalStatusDefinedType: DefinedTypeBag;
  defaultAdultMaritalStatus: DefinedValueBag;
  defaultChildMaritalStatus: DefinedValueBag;
  showEmail: boolean;
  showEmailOptOut: boolean;
  showCell: boolean;
  showSMSEnabled: boolean;
  phoneType: DefinedValueBag;
  showAddress: boolean;
  existingPersonPhoneCantBeMessaged: boolean;
  adultAttributes: AttributeBag[];
  childAttributes: AttributeBag[];
  abilityLevelDefinedType: DefinedTypeBag;
  abilityLevelAttribute: AttributeBag;
  gradeDefinedType: DefinedTypeBag;
  graduationYear: number;
  existingPerson: PersonBag;
  emptyPerson: PersonBag;
  existingPersonPhoneNumber: PhoneNumberBag;
  emptyPersonPhoneNumber: PhoneNumberBag;
  Groups: GroupBag[];
  GroupStartDOBAttribute: AttributeBag;
  GroupEndDOBAttribute: AttributeBag;
  GroupAbilityAttribute: AttributeBag;
  GroupGradeAttribute: AttributeBag;
}