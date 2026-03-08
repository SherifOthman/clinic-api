-- Fix duplicate popular plans - only Professional should be popular
UPDATE SubscriptionPlans 
SET IsPopular = 0 
WHERE Name = 'Basic';

UPDATE SubscriptionPlans 
SET IsPopular = 1 
WHERE Name = 'Professional';
