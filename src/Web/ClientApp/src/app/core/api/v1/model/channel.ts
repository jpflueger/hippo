/**
 * Hippo.Web
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 1.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { App } from './app';
import { Revision } from './revision';
import { ChannelRevisionSelectionStrategy } from './channelRevisionSelectionStrategy';
import { DomainEvent } from './domainEvent';
import { EnvironmentVariable } from './environmentVariable';
import { Certificate } from './certificate';


export interface Channel { 
    created?: string;
    createdBy?: string | null;
    lastModified?: string;
    lastModifiedBy?: string | null;
    id?: string;
    name?: string | null;
    domain?: string | null;
    revisionSelectionStrategy?: ChannelRevisionSelectionStrategy;
    rangeRule?: string | null;
    activeRevisionId?: string | null;
    activeRevision?: Revision;
    certificateId?: string | null;
    certificate?: Certificate;
    portId?: number;
    appId?: string;
    app?: App;
    readonly environmentVariables?: Array<EnvironmentVariable> | null;
    domainEvents?: Array<DomainEvent> | null;
}

